using System.Threading.Tasks;
using Chat.Client.Database;
using Chat.Client.Database.Repositories;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerRequestKey)]
public class KeyRequestHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var request = inPacket.Decode<ServerRequestKey>();
        var repo = DatabaseManager.GetRepository<ChannelRepository>();
        var channelData = repo.GetChannel(request.Channel);
        if (channelData == null) return Task.CompletedTask;
        if (channelData.Key.Length == 0) return Task.CompletedTask;

        using var packet = new OutPacket(ClientHeader.ClientResponseKey);
        packet.Encode(new ClientResponseKey {Channel = request.Channel, Key = channelData.Key});
        session.Send(packet);

        return Task.CompletedTask;
    }
}