using System.Threading;
using System.Threading.Tasks;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerMessage)]
public class MessageHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var data = inPacket.Decode<ServerMessage>();
        if (data.Message.Attachment == null)
        {
            session.ViewModel.AddMessage(data.Message);
            return Task.CompletedTask;
        }

        var fileData = new byte[data.Message.Attachment.Length - 1];
        System.Buffer.BlockCopy(data.Message.Attachment, 1, fileData, 0, fileData.Length);

        switch (data.Message.Attachment[0])
        {
            case 1: // mp3
                data.Message.Text = $"{data.Message.Text} 재생";
                session.ViewModel.AddMessage(data.Message);
                session.ViewModel.MusicThread?.Interrupt();
                session.ViewModel.MusicThread = new Thread(() => session.ViewModel.PlayMusic(fileData));
                session.ViewModel.MusicThread.Start();
                break;
        }

        return Task.CompletedTask;
    }
}