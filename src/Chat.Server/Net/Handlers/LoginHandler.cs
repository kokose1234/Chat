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
        uint userId;
        dynamic self;

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

            
            if (!BCrypt.Net.BCrypt.EnhancedVerify(request.Password, account[0].password))
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

            if (!ChatServer.Instance.Clients.ContainsKey(session.Id.ToString())) return;
            var client = ChatServer.Instance.Clients[session.Id.ToString()];
            client.Id = account[0].id;
            client.Username = account[0].username;
            client.Name = account[0].name;
            client.IsAdmin = account[0].admin == 1;
            userId = account[0].id;
            self = account[0];

            session.Send(packet);

            ChatServer.Instance.AddClient(client);
        }

        using (var mutex = await DatabaseManager.Mutex.ReaderLockAsync())
        {
            using var packet = new OutPacket(ServerHeader.ServerUserList);
            var channelUsers = await DatabaseManager.Factory.Query("accounts")
                                                    .Join("channel_users", "accounts.id", "channel_users.user_id")
                                                    .Where("channel_users.user_id", userId)
                                                    .Distinct()
                                                    .GetAsync();
            var friends = (await DatabaseManager.Factory.Query("accounts")
                                                .Join("friends", "accounts.id", "friends.friend_user_id")
                                                .Where("friends.user_id", userId)
                                                .Distinct()
                                                .GetAsync()).ToImmutableArray();
            var users = channelUsers.Concat(friends).ToList();
            users.Add(self);
            var userList = new ServerUserList();

            userList.Users.AddRange(users.DistinctBy(x => x.id).Select(user => new UserInfo
            {
                Id = user.id,
                Name = user.name
            }));

            packet.Encode(userList);
            session.Send(packet);
        }

        {
            using var packet = new OutPacket(ServerHeader.ServerChannelSync);
            var channels = ChatServer.Instance.GetChannels(userId);
            var data = new ServerChannelSync();

            data.Channels.AddRange(channels.Select(channel => new ChannelInfo
            {
                Id = channel.Id,
                Name = channel.Name,
                IsSecret = channel.IsSecret
            }));

            packet.Encode(data);
            session.Send(packet);
        }
    }
}