using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientSyncMusic)]
public class SyncMusicHandler : AbstractHandler
{
    internal override Task Handle(ChatSession session, InPacket inPacket)
    {
        var data = inPacket.Decode<ClientSyncMusic>();

        //TODO: 채팅방 참가자만 보내기
        foreach (var kvp in ChatServer.Clients)
        {
            if (kvp.Value == session.Client) continue;

            var packet = new OutPacket(ServerHeader.ServerSyncMusic);
            var serverSyncMusic = new ServerSyncMusic
            {
                Channel = data.Channel,
                Data = data.Data
            };
            packet.Encode(serverSyncMusic);
            kvp.Value.Session.Send(packet);
        }

        return Task.CompletedTask;
    }
}