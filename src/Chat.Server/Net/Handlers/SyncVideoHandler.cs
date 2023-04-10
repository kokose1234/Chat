using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientSyncVideo)]
public class SyncVideoHandler : AbstractHandler
{
    internal override Task Handle(ChatSession session, InPacket inPacket)
    {
        var data = inPacket.Decode<ClientSyncMusic>();
        var channel = ChatServer.Instance.GetChannel(data.Channel);
        var user = channel?.GetUser(session.Client.Id);

        if (channel != null && user != null)
        {
            var packet = new OutPacket(ServerHeader.ServerSyncVideo);
            var serverSyncVideo = new ServerSyncVideo
            {
                Channel = data.Channel,
                Data = data.Data
            };
            packet.Encode(serverSyncVideo);

            channel.Broadcast(packet, user);
        }

        return Task.CompletedTask;
    }
}