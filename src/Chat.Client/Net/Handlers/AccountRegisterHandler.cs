using System.Threading.Tasks;
using Chat.Client.Data.Types;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerAccountRegister)]
internal class AccountRegisterHandler : AbstractHandler
{
    internal override async Task Handle(ChatClient session, InPacket inPacket)
    {
        var request = inPacket.Decode<ServerAccountRegister>();

        switch (request.Result)
        {
            case ServerAccountRegister.RegisterResult.Success:
                session.ViewModel.LoginMessage = new()
                {
                    Type = LoginMessageType.Success,
                    Message = "회원가입에 성공하였습니다."
                };
                await Task.Delay(3000);
                session.ViewModel.LoginMessage = new();
                break;
            case ServerAccountRegister.RegisterResult.FailDuplicateMac:
                session.ViewModel.LoginMessage = new()
                {
                    Type = LoginMessageType.Error,
                    Message = "이미 등록된 MAC 주소입니다."
                };
                await Task.Delay(3000);
                session.ViewModel.LoginMessage = new();
                break;
            case ServerAccountRegister.RegisterResult.FailDuplicateUsesrname:
                session.ViewModel.LoginMessage = new()
                {
                    Type = LoginMessageType.Error,
                    Message = "이미 등록된 사용자 이름입니다."
                };
                await Task.Delay(3000);
                session.ViewModel.LoginMessage = new();
                break;
            case ServerAccountRegister.RegisterResult.FailBlocked:
                session.ViewModel.LoginMessage = new()
                {
                    Type = LoginMessageType.Error,
                    Message = "차단된 MAC 주소입니다."
                };
                await Task.Delay(3000);
                session.ViewModel.LoginMessage = new();
                break;
            case ServerAccountRegister.RegisterResult.FailUnkown:
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