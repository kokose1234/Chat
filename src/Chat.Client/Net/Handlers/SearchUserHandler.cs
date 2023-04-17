using System.Linq;
using System.Threading.Tasks;
using Chat.Client.Data;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;
using DynamicData;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerUserSearchResult)]
public class SearchUserHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var request = inPacket.Decode<ServerUserSearchResult>();
        session.ViewModel.SearchUsers.Clear();
        session.ViewModel.SearchUsers.AddRange(request.UserMaps.Select(x => new UserSearchResult
        {
            Id = x.Key,
            Username = $"@{x.Value.Username}",
            Nickname = x.Value.Nickname,
            Message = x.Value.Message,
            Avatar = x.Value.Avatar,
        }));

        return Task.CompletedTask;
    }
}