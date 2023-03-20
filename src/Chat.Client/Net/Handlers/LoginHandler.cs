using System.Threading.Tasks;
using Chat.Client.Data.Types;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerLogin)]
internal class LoginHandler : AbstractHandler
{
    internal override async Task Handle(ChatClient session, InPacket inPacket)
    {
        var data = inPacket.Decode<ServerLogin>();

        switch (data.Result)
        {
            case ServerLogin.LoginResult.Success:
                session.ViewModel.IsLogined = true;
                break;
            case ServerLogin.LoginResult.FailedDuplicateUser:
                session.ViewModel.LoginMessage = new()
                {
                    Type = LoginMessageType.Error,
                    Message = "이미 로그인된 사용자입니다."
                };
                await Task.Delay(3000);
                session.ViewModel.LoginMessage = new();
                break;
            case ServerLogin.LoginResult.FailedWrongInfo:
                session.ViewModel.LoginMessage = new()
                {
                    Type = LoginMessageType.Error,
                    Message = "아이디 또는 비밀번호가 틀렸습니다."
                };
                await Task.Delay(3000);
                session.ViewModel.LoginMessage = new();
                break;
            case ServerLogin.LoginResult.FailedBlocked:
                session.ViewModel.LoginMessage = new()
                {
                    Type = LoginMessageType.Error,
                    Message = "차단된 사용자입니다."
                };
                await Task.Delay(3000);
                session.ViewModel.LoginMessage = new();
                break;
            case ServerLogin.LoginResult.FailedUnkown:
                session.ViewModel.LoginMessage = new()
                {
                    Type = LoginMessageType.Error,
                    Message = "알 수 없는 오류가 발생하였습니다."
                };
                await Task.Delay(3000);
                session.ViewModel.LoginMessage = new();
                break;
        }
    }
}