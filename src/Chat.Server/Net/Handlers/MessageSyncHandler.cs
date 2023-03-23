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

        if (request.LastMessageIds.Count == 0)
        {
            packet.Encode(response);
            session.Send(packet);
            return;
        }

        foreach (var (channel, lastMessage) in request.LastMessageIds)
        {
            var messages = await DatabaseManager.Factory.Query("messages")
                                                .Where("id", ">", lastMessage.ToString())
                                                .GetAsync();

            foreach (var message in messages)
            {
                response.Messages.Add(new Message
                {

                });
            }
        }
    }
}