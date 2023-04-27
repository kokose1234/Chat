using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientUserSearch)]
public class SearchUserHandler : AbstractHandler
{
    internal override Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientUserSearch>();
        var response = new ServerUserSearchResult();
        var users = ChatServer.Instance.GetAllUsers().Where(x => x.Username.Contains(request.SearchTerm) || x.Nickname.Contains(request.SearchTerm));

        foreach (var user in users)
        {
            if (user.Id == session.Client.Id) continue;
            var info = new ServerUserSearchResult.User { Username = user.Username, Nickname = user.Nickname, Message = user.Message, LastAvatarUpdate = user.LastAvatarUpdate };
            response.UserMaps.Add(user.Id, info);
        }

        using var packet = new OutPacket(ServerHeader.ServerUserSearchResult);
        packet.Encode(response);
        session.Send(packet);

        return Task.CompletedTask;
    }
}