using System.Linq;
using System.Threading.Tasks;
using Chat.Client.Models;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerCreateChannel)]
public class CreateChannelHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var request = inPacket.Decode<ServerCreateChannel>();

        foreach (var user in request.Users.Where(user => session.ViewModel.Users.All(x => x.Id != user.Id)))
        {
            session.ViewModel.Users.Add(user);
        }

        session.ViewModel.Channels.Add(new Channel
        {
            Id = request.Channel.Id,
            Name = request.Channel.Name,
            Description = string.Empty,
        });

        return Task.CompletedTask;
    }
}