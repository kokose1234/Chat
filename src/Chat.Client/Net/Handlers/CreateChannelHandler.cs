using System.Linq;
using System.Threading.Tasks;
using Chat.Client.Database;
using Chat.Client.Database.Repositories;
using Chat.Client.Models;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;
using LiteDB;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerCreateChannel)]
public class CreateChannelHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var request = inPacket.Decode<ServerCreateChannel>();
        var repo = DatabaseManager.GetRepository<ChannelRepository>();

        foreach (var user in request.Users.Where(user => session.ViewModel.Users.All(x => x.Id != user.Id)))
        {
            session.ViewModel.Users.Add(user);
        }

        session.ViewModel.Channels.Add(new Channel
        {
            Id = request.Channel.Id,
            Name = request.Channel.Name,
            Description = string.Empty,
            IsSecret = request.Channel.IsSecret,
            Key = request.Channel.Key
        });

        repo.AddChannel(new()
        {
            Id = ObjectId.NewObjectId(),
            ChannelId = request.Channel.Id,
            Key = request.Channel.Key,
            IsSecret = request.Channel.IsSecret
        });

        return Task.CompletedTask;
    }
}