﻿using System.Linq;
using System.Threading.Tasks;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerRemoveFriend)]
public class RemoveFriendHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var request = inPacket.Decode<ServerRemoveFriend>();
        if (session.ViewModel.Users.Any(x => x.Id == request.User.Id))
        {
            session.ViewModel.Users.First(x => x.Id == request.User.Id).IsFriend = false;
        }

        return Task.CompletedTask;
    }
}