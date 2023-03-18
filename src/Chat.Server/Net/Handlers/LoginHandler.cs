using Chat.Common.Net.Packet.Header;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Packet.Data.Server;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientLogin)]
public class LoginHandler : AbstractHandler
{
    internal override void Handle(ChatSession session, InPacket packet)
    {
        var outPacket = new OutPacket(ServerHeader.ServerLogin);
        outPacket.Encode(new ServerLogin() {Result = ServerLogin.LoginResult.FailedBlocked});
        session.Send(outPacket);
    }
}