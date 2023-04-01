using System.Linq;
using System.Threading.Tasks;
using Chat.Client.Models;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;
using DynamicData;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerChannelSync)]
public class ChannelSyncHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var data = inPacket.Decode<ServerChannelSync>();

        session.ViewModel.Channels.Clear();
        session.ViewModel.Channels.AddRange(data.Channels.Select(x => new Channel
        {
            Id = x.Id,
            Name = x.Name,
            Description = string.Empty,
        }));

        return Task.CompletedTask;
    }
}