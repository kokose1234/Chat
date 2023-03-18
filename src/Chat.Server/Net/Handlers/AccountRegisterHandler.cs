using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientAccountRegister)]
internal class AccountRegisterHandler : AbstractHandler
{
    internal override void Handle(ChatSession session, InPacket inPacket) //TODO: 맥주소 밴
    {
        var packet = new OutPacket(ServerHeader.ServerAccountRegister);

        try
        {
            var request = inPacket.Decode<ClientAccountRegister>();

            var account = DatabaseManager.Factory.Query("accounts").Get().ToArray();

            if (account.FirstOrDefault(x => x.registered_mac == request.MacAddress) != null)
            {
                packet.Encode(new ServerAccountRegister {Result = ServerAccountRegister.RegisterResult.FailDuplicateMac});
                session.Send(packet);
                return;
            }

            if (account.FirstOrDefault(x => x.username == request.UserName) != null)
            {
                packet.Encode(new ServerAccountRegister {Result = ServerAccountRegister.RegisterResult.FailDuplicateUsesrname});
                session.Send(packet);
                return;
            }

            DatabaseManager.Factory.Query("accounts").Insert(new
            {
                username = request.UserName,
                password = request.Password,
                registered_mac = request.MacAddress
            });
            packet.Encode(new ServerAccountRegister {Result = ServerAccountRegister.RegisterResult.Success});
            session.Send(packet);
        }
        catch (Exception ex)
        {
            packet.Encode(new ServerAccountRegister {Result = ServerAccountRegister.RegisterResult.FailUnkown});
            session.Send(packet);
            Console.WriteLine(ex);
        }
    }
}