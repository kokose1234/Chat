using Chat.Common.Net.Packet.Header;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerLogin)]
internal class LoginHandler : AbstractHandler
{
    internal override void Handle(ChatClient session, InPacket packet)
    {
        var data = packet.Decode<ServerLogin>();

        if (data.Result != ServerLogin.LoginResult.Success)
        {
            session.ViewModel.IsLogined = true;
            // session.ViewModel.ErrorMessage = "실패";
        }
    }
}