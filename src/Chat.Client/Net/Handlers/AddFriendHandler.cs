using System.Linq;
using System.Threading.Tasks;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerAddFriend)]
public class AddFriendHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var request = inPacket.Decode<ServerAddFriend>();
        if (session.ViewModel.Users.Any(x => x.Id == request.User.Id))
        {
            session.ViewModel.Users.First(x => x.Id == request.User.Id).IsFriend = true;
        }
        else
        {
            session.ViewModel.Users.Add(request.User);
        }

        return Task.CompletedTask;
    }
}