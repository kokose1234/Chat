using System.IO;
using System.Threading.Tasks;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerMessageSync)]
public class MessageSyncHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        File.WriteAllBytes("test2.bin", inPacket.Buffer);
        var request = inPacket.Decode<ServerMessageSync>();

        return Task.CompletedTask;
    }
}