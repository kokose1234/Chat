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

        {
            using var packet = new OutPacket(ServerHeader.ServerUserList);
            var users = await DatabaseManager.Factory.SelectAsync("CALL getRelatedUsers(@userId)", new {userId = userId});
            var userList = new ServerUserList();

            userList.Users.AddRange(users.Select(user => new UserInfo
            {
                Id = user.id,
                Name = user.name,
                Message = user.message
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

        {
            var channels = ChatServer.Instance.GetChannels(userId);
            foreach (var channel in channels)
            {
                if (!channel.IsSecret) continue;
                if (channel.GetUser(userId) == null) continue;

                if (channel.Users.Where(x => x.Id != userId && x.Client != null).Any(x => x.KeyRequested))
                {
                    using var packet = new OutPacket(ServerHeader.ServerRequestKey);
                    packet.Encode(new ServerRequestKey {Channel = channel.Id});
                    session.Send(packet);
                }
            }
        }

        {
            using var packet = new OutPacket(ServerHeader.ServerResponseImage);
            var packetData = new ServerResponseImage {type = ServerResponseImage.Type.Profile};
            var data = new ServerResponseImage.Data
            {
                Id = self.id,
                Image = self.avatar,
                Update = self.avatar_update_date
            };

            packetData.Datas.Add(data);
            packet.Encode(packetData);
            session.Send(packet);
        }
    }
}