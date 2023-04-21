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

[PacketHandler(ServerHeader.ServerChannelSync)]
public class ChannelSyncHandler : AbstractHandler
{
    internal override async Task Handle(ChatClient session, InPacket inPacket)
    {
        var data = inPacket.Decode<ServerChannelSync>();
        var repo = DatabaseManager.GetRepository<ChannelRepository>();
        using var mutex = await repo.Mutex.WriterLockAsync();

        session.ViewModel.Channels.Clear();

        foreach (var channel in data.Channels)
        {
            var result = repo.GetChannel(channel.Id);
            var info = new Channel
            {
                Id = channel.Id,
                Name = channel.Name,
                Description = string.Empty,
                IsSecret = channel.IsSecret
            };

            if (result == null)
            {
                result = new()
                {
                    Id = ObjectId.NewObjectId(),
                    ChannelId = channel.Id
                };
                repo.AddChannel(result);
                RequestKey();
            }
            else
            {
                if (channel.IsSecret)
                {
                    if (result.Key.Length != 0)
                    {
                        info.Key = result.Key;
                    }
                    else
                    {
                        RequestKey();
                    }
                }
            }

            session.ViewModel.Channels.Add(info);
        }
    }

    private void RequestKey() { }
}