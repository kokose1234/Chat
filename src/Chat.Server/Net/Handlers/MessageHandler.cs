using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientMessage)]
public class MessageHandler : AbstractHandler
{
    internal override Task Handle(ChatSession session, InPacket inPacket)
    {
        var data = inPacket.Decode<ClientMessage>();
        var channel = ChatServer.Instance.GetChannel(data.Channel);

        channel?.OnMessage(session.Client, data);

        return Task.CompletedTask;
    }
}