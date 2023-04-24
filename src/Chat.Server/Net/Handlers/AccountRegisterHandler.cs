using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Data;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientAccountRegister)]
internal class AccountRegisterHandler : AbstractHandler
{
    internal override async Task Handle(ChatSession session, InPacket inPacket) //TODO: 맥주소 밴
    {
        var packet = new OutPacket(ServerHeader.ServerAccountRegister);

        try
        {
            var request = inPacket.Decode<ClientAccountRegister>();

            using (var mutex = await DatabaseManager.Mutex.ReaderLockAsync())
            {
                var account = (await DatabaseManager.Factory.Query("accounts").GetAsync()).ToArray();

#if !DEBUG
                if (account.FirstOrDefault(x => x.registered_mac == request.MacAddress) != null)
                {
                    packet.Encode(new ServerAccountRegister
                        {Result = ServerAccountRegister.RegisterResult.FailDuplicateMac});
                    session.Send(packet);
                    return;
                }
#endif

                if (account.FirstOrDefault(x => x.username == request.UserName) != null)
                {
                    packet.Encode(new ServerAccountRegister
                        {Result = ServerAccountRegister.RegisterResult.FailDuplicateUsesrname});
                    session.Send(packet);
                    return;
                }
            }

            using (var mutex = await DatabaseManager.Mutex.WriterLockAsync())
            {
                var id = await DatabaseManager.Factory.Query("accounts").InsertAsync(new
                {
                    username = request.UserName,
                    password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
                    name = request.UserName,
                    registered_mac = request.MacAddress,
                    avatar = Constants.DefaultProfileImage,
                    avatar_update_date = DateTime.Now.Ticks
                });
                ChatServer.Instance.AddUser(new User((uint) id, request.UserName, request.UserName, null, (ulong) DateTime.Now.Ticks));
            }

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