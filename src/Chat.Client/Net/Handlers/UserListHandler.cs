using System.Threading.Tasks;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerUserList)]
public class UserListHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var data = inPacket.Decode<ServerUserList>();
        session.ViewModel.Users.AddRange(data.Users);

        return Task.CompletedTask;
    }
}