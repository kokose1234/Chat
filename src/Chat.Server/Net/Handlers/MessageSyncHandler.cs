using System.Collections.Immutable;
using Chat.Common.Data;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientMessageSync)]
public class MessageSyncHandler : AbstractHandler
{
    internal override async Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientMessageSync>();
        using var packet = new OutPacket(ServerHeader.ServerMessageSync);
        var response = new ServerMessageSync();

        using (var mutex = await DatabaseManager.Mutex.ReaderLockAsync())
        {
            var unsentMessages = (await DatabaseManager.Factory.Query("channel_users")
                                                       .Join("messages", "channel_users.channel_id", "messages.channel_id")
                                                       .Select("messages.*")
                                                       .Where("channel_users.user_id", session.Client.Id.ToString())
                                                       .Where("messages.id", ">", request.LastMessageId.ToString())
                                                       .GetAsync()).ToImmutableArray();
            var messages = unsentMessages.Select(x => new Message
            {
                Id = x.id,
                ChannelId = x.channel_id,
                Sender = x.user_id,
                Date = x.date,
                Text = x.message,
                Attachment = (byte[]) x.attachment,
            });

            response.Messages.AddRange(messages);
        }

        packet.Encode(response);
        session.Send(packet);
    }
}