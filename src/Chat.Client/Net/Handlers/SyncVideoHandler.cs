using System.Threading.Tasks;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerSyncVideo)]
public class SyncVideoHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        if (session.ViewModel.VideoPlayer == null) return Task.CompletedTask;

        var request = inPacket.Decode<ServerSyncVideo>();
        if (request.Channel == session.ViewModel.SelectedChannel?.Id)
        {
            switch (request.Data)
            {
                case int.MinValue + 1:
                    session.ViewModel.VideoPlayer.Resume();
                    break;
                case int.MinValue + 2:
                    session.ViewModel.VideoPlayer.Pause();
                    break;
                default:
                    session.ViewModel.VideoPlayer.Position = request.Data * -1;
                    break;
            }
        }

        return Task.CompletedTask;
    }
}