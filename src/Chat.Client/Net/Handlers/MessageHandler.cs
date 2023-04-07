using System.Threading.Tasks;
using Chat.Client.Data.Types;
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
        var fileName = data.Message.Text;
        System.Buffer.BlockCopy(data.Message.Attachment, 1, fileData, 0, fileData.Length);

        switch ((AttachmentType) data.Message.Attachment[0])
        {
            case AttachmentType.Music:
                data.Message.Text = $"{data.Message.Text} 재생";
                session.ViewModel.AddMessage(data.Message);
                session.ViewModel.PlayMusic(fileData);
                break;
            case AttachmentType.Video:
                data.Message.Text = $"{data.Message.Text} 재생";
                session.ViewModel.AddMessage(data.Message);
                session.ViewModel.PlayVideo(fileName, fileData);
                break;
        }

        return Task.CompletedTask;
    }
}