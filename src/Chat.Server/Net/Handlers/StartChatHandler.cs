using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Server.Database;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientStartChat)]
public class StartChatHandler : AbstractHandler
{
    internal override async Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientStartChat>();
        if (ChatServer.Instance.GetChannels(session.Client.Id).Any(x => x.Users.Any(x => x.Id == request.Id))) return;

        using (var mutex = await DatabaseManager.Mutex.WriterLockAsync())
        {
            //TODO: Add
        }
    }
}