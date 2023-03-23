using Chat.Common.Data;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientMessageSync)]
public class MessageSyncHandler : AbstractHandler
{
    internal override async Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientMessageSync>();
        using var packet = new OutPacket(ServerHeader.ServerMessageSync);
        var response = new ServerMessageSync();

        using (var mutex = await DatabaseManager.Mutex.ReaderLockAsync())
        {
            // var channels = await DatabaseManager.Factory.Query("channel-users").Where()
        }
    }
}