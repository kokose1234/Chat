using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Database;

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
            var users = ChatServer.Instance.GetAllUsers().Where(x => x.Username.Contains(request.SearchTerm) || x.Nickname.Contains(request.SearchTerm));

            foreach (var user in users)
            {
                if (user.Id == session.Client.Id) continue;
                var info = new ServerUserSearchResult.User {Username = user.Username, Nickname = user.Nickname, Message = user.Message, LastAvatarUpdate = user.LastAvatarUpdate};
                response.UserMaps.Add(user.Id, info);
            }
        }

        using var packet = new OutPacket(ServerHeader.ServerUserSearchResult);
        packet.Encode(response);
        session.Send(packet);
    }
}