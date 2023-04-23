using System.Linq;
using System.Threading.Tasks;
using Chat.Client.Database;
using Chat.Client.Database.Repositories;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerDeliverKey)]
public class KeyDeliverHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var response = inPacket.Decode<ServerDeliverKey>();
        var repo = DatabaseManager.GetRepository<ChannelRepository>();
        var channelData = repo.GetChannel(response.Channel);
        if (channelData == null) return Task.CompletedTask;

        channelData.Key = response.Key;
        session.ViewModel.Channels.First(x => x.Id == response.Channel).Key = response.Key;
        repo.UpdateChannel(channelData);

        //TODO: 이미 온 메시지 복호화

        return Task.CompletedTask;
    }
}