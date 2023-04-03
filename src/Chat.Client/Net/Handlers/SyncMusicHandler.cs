using System.Threading.Tasks;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerSyncMusic)]
public class SyncMusicHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var request = inPacket.Decode<ServerSyncMusic>();
        if (request.Channel == session.ViewModel.SelectedChannel?.Id)
        {
            switch (request.Data)
            {
                case int.MinValue + 1:
                    session.ViewModel.ResumeMusic();
                    break;
                case int.MinValue + 2:
                    session.ViewModel.PauseMusic();
                    break;
                default:
                    session.ViewModel.MusicPosition = request.Data * -1;
                    break;
            }
        }

        return Task.CompletedTask;
    }
}