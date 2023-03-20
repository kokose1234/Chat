using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientPong)]
public class PongHandler : AbstractHandler
{
    internal override Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientPong>();
        session.Client.LastPongTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        if ((request.Key ^ (ulong) ClientHeader.ClientPong) != (ulong) session.Client.LastPingKey)
        {
            session.Close();
        }

        return Task.CompletedTask;
    }
}