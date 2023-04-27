using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

//TODO: 상대방도 친구 삭제
[PacketHandler(ClientHeader.ClientRemoveFriend)]
public class RemoveFriendHandler : AbstractHandler
{
    internal override async Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientRemoveFriend>();
        using var packet = new OutPacket(ServerHeader.ServerRemoveFriend);
        var account = (await DatabaseManager.Factory.Query("accounts").Where("id", request.Id).GetAsync()).ToArray();
        if (account.Length == 0) return;
        var data = new ServerRemoveFriend
        {
            User = new()
            {
                Id = account[0].id,
                Name = account[0].name
            }
        };

        await DatabaseManager.Factory.Query("friends").Where(new
        {
            user_id = session.Client.Id,
            friend_user_id = request.Id
        }).DeleteAsync();
        packet.Encode(data);
        session.Send(packet);
    }
}