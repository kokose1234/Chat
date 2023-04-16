﻿using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientAddFriend)]
public class AddFriendHandler : AbstractHandler
{
    internal override async Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientAddFriend>();
        using (var mutex = await DatabaseManager.Mutex.WriterLockAsync())
        {
            using var packet = new OutPacket(ServerHeader.ServerAddFriend);
            var account = (await DatabaseManager.Factory.Query("accounts").Where("id", request.Id).GetAsync()).ToArray();
            if (account.Length == 0) return;
            var data = new ServerAddFriend
            {
                User = new()
                {
                    Id = account[0].id,
                    Name = account[0].name,
                    IsFriend = true,
                    IsMe = false
                }
            };

            await DatabaseManager.Factory.Query("friends").InsertAsync(new
            {
                user_id = session.Client.Id,
                friend_user_id = request.Id
            });
            packet.Encode(data);
            session.Send(packet);
        }
    }
}