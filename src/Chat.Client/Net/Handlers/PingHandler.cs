using System.Threading.Tasks;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerPing)]
public class PingHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var request = inPacket.Decode<ServerPing>();
        using var packet = new OutPacket(ClientHeader.ClientPong);
        var pong = new ClientPong {Key = (ulong) request.Key ^ (ulong) ClientHeader.ClientPong};

        packet.Encode(pong);
        session.Send(packet);

        return Task.CompletedTask;
    }
}