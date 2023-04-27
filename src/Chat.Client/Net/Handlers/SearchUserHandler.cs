using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Client.Data;
using Chat.Client.Database.Repositories;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using DynamicData;
using static Chat.Common.Packet.Data.Client.ClientRequestImage;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerUserSearchResult)]
public class SearchUserHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var response = inPacket.Decode<ServerUserSearchResult>();
        var userList = new List<uint>(); // 프로필 이미지를 새로 받아와야 하는 유저 리스트
        var repo = Database.DatabaseManager.GetRepository<UserRepository>();

        foreach (var (id, user) in response.UserMaps)
        {
            var userEntity = repo.GetUser(id);

            if (userEntity == null)
            {
                userList.Add(id);
                continue;
            }

            if (userEntity.LastAvatarUpdate < user.LastAvatarUpdate) userList.Add(id);
        }

        if (userList.Count > 0)
        {
            using var packet = new OutPacket(ClientHeader.ClientRequestImage);
            var request = new ClientRequestImage
            {
                type = Type.Profile,
                Ids = userList.ToArray()
            };

            packet.Encode(request);
            session.Send(packet);
        }

        session.ViewModel.SearchUsers.Clear();
        session.ViewModel.SearchUsers.AddRange(response.UserMaps.Select(x => new UserSearchResult
        {
            Id = x.Key,
            Username = $"@{x.Value.Username}",
            Nickname = x.Value.Nickname,
            Message = x.Value.Message
        }).OrderBy(x => x.Nickname));

        return Task.CompletedTask;
    }
}