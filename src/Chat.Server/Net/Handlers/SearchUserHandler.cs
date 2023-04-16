using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientUserSearch)]
public class SearchUserHandler : AbstractHandler
{
    internal override async Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientUserSearch>();
        var response = new ServerUserSearchResult();

        using (var mutex = await DatabaseManager.Mutex.ReaderLockAsync())
        {
            var users = (await DatabaseManager.Factory.Query("accounts").WhereLike("name", $"%{request.SearchTerm}%").OrWhereLike("username", $"%{request.SearchTerm}%").GetAsync()).ToArray();

            foreach (var user in users)
            {
                if (user.id == session.Client.Id) continue;
                var info = new ServerUserSearchResult.User {Username = user.username, Nickname = user.name, Message = user.message, Avatar = user.avatar};
                response.UserMaps.Add(user.id, info);
            }
        }

        using var packet = new OutPacket(ServerHeader.ServerUserSearchResult);
        packet.Encode(response);
        session.Send(packet);
    }
}