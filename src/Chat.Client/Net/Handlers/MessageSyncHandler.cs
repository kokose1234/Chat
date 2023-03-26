using System.Threading.Tasks;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerMessageSync)]
public class MessageSyncHandler : AbstractHandler
{
    //TODO: LiteDB에 저장
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var response = inPacket.Decode<ServerMessageSync>();

        foreach (var message in response.Messages)
        {
            message.IsMyMessage = message.Sender == session.UserId;
            session.ViewModel.AddMessage(message);
        }

        return Task.CompletedTask;
    }
}