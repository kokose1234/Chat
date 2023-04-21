using System.Threading.Tasks;
using Chat.Client.Database;
using Chat.Client.Database.Repositories;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerUserList)]
public class UserListHandler : AbstractHandler
{
    internal override async Task Handle(ChatClient session, InPacket inPacket)
    {
        var data = inPacket.Decode<ServerUserList>();
        session.ViewModel.Users.AddRange(data.Users);

        using var packet = new OutPacket(ClientHeader.ClientMessageSync);
        var repo = DatabaseManager.GetRepository<ConfigRepository>();
        using var mutex = await repo.Mutex.ReaderLockAsync();
        var request = new ClientMessageSync
        {
            LastMessageId = repo.GetConfig().LastMessage
        };

        packet.Encode(request);
        session.Send(packet);
    }
}