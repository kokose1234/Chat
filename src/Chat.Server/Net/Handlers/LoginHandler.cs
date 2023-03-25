using System.Collections.Immutable;
using Chat.Common.Data;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientLogin)]
public class LoginHandler : AbstractHandler
{
    internal override async Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientLogin>();

        using (var mutex = await DatabaseManager.Mutex.ReaderLockAsync())
        {
            using var packet = new OutPacket(ServerHeader.ServerLogin);
            var account = (await DatabaseManager.Factory.Query("accounts").Where("username", request.UserName).GetAsync())
                .ToImmutableArray();

            if (account.Length == 0)
            {
                packet.Encode(new ServerLogin {Result = ServerLogin.LoginResult.FailedWrongInfo});
                session.Send(packet);
                return;
            }

            if (account[0].password != request.Password)
            {
                packet.Encode(new ServerLogin {Result = ServerLogin.LoginResult.FailedWrongInfo});
                session.Send(packet);
                return;
            }

            packet.Encode(new ServerLogin
            {
                Id = account[0].id,
                Result = ServerLogin.LoginResult.Success,
                Name = account[0].name,
                IsAdmin = account[0].admin == 1,
            });

            if (!ChatServer.Clients.ContainsKey(session.Id.ToString())) return;
            var client = ChatServer.Clients[session.Id.ToString()];
            client.Id = account[0].id;
            client.Username = account[0].username;
            client.Name = account[0].name;
            client.IsAdmin = account[0].admin == 1;

            session.Send(packet);
        }

        //TODO: 친구, 같은 대화방 사람만 보내기
        using (var mutex = await DatabaseManager.Mutex.ReaderLockAsync())
        {
            using var packet = new OutPacket(ServerHeader.ServerUserList);
            var users = (await DatabaseManager.Factory.Query("accounts").WhereNot("username", request.UserName).GetAsync()).ToImmutableArray();
            var userList = new ServerUserList();

            userList.Users.AddRange(users.Select(user => new UserInfo
            {
                Id = user.id,
                Name = user.name,
            }));

            packet.Encode(userList);
            session.Send(packet);
        }
    }
}