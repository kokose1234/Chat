using Chat.Common.Data;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientMessage)]
public class MessageHandler : AbstractHandler
{
    internal override Task Handle(ChatSession session, InPacket inPacket)
    {
        var data = inPacket.Decode<ClientMessage>();

        foreach (var kvp in ChatServer.Clients)
        {
            var packet = new OutPacket(ServerHeader.ServerMessage);
            var message = new Message
            {
                ChannelId = 1,
                Sender = session.Client.Id,
                Text = data.Message,
                Attachment = data.Attachment,
                Date = DateTime.Now,
                Encrypted = data.IsEncrypted,
                IsMyMessage = kvp.Value == session.Client
            };
            var serverMessage = new ServerMessage {Message = message};
            packet.Encode(serverMessage);
            kvp.Value.Session.Send(packet);
        }

        return Task.CompletedTask;
    }
}