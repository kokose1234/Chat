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
        if (request.LastMessageIds.Count == 0) return;

        using var packet = new OutPacket(ServerHeader.ServerMessageSync);
        var response = new ServerMessageSync();

        foreach (var (channelId, lastMessageId) in request.LastMessageIds)
        {
            var unsentMessages = await DatabaseManager.Factory.Query("messages")
                                                      .Where("channel_id", channelId)
                                                      .Where("id", ">", lastMessageId)
                                                      .GetAsync();
            var messages = unsentMessages.Select(x => new Message
            {
                Id = x.id,
                ChannelId = x.channel_id,
                Sender = x.user_id,
                Date = x.date,
                Text = x.message,
                Attachment = (byte[]) x.attachment,
            });
            var data = new ServerMessageSync.Messages {Channel = channelId};
            data.messages.AddRange(messages);
            response.messages.Add(channelId, data);
        }

        packet.Encode(response);
        session.Send(packet);
    }
}