using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Database;
using Chat.Server.Tools;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientStartChat)]
public class StartChatHandler : AbstractHandler
{
    internal override async Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientStartChat>();
        var self = ChatServer.Instance.GetUser(session.Client.Id);
        var users = ChatServer.Instance.GetUsers(request.UserIds);
        if (users.Count == 0 || self == null) return;
        users.Add(self);

        //TODO: 나중에 지울수도
        if (ChatServer.Instance.GetChannels(session.Client.Id).Any(channel => channel.Users.Any(user => request.UserIds.Contains(user.Id)))) return;

        using var packet = new OutPacket(ServerHeader.ServerCreateChannel);
        var data = new ServerCreateChannel();
        var channelName = string.IsNullOrEmpty(request.Name) ? string.Join(", ", users.Select(x => x.Nickname)) : request.Name;
        var key = request.IsSecret ? Util.GetRandomBytes(32) : null;
        var id = await DatabaseManager.Factory.Query("channels").InsertGetIdAsync<uint>(new
        {
            name = channelName,
            is_secret = request.IsSecret ? 1 : 0,
        });

        foreach (var user in users)
        {
            await DatabaseManager.Factory.Query("channel_users").InsertAsync(new
            {
                channel_id = id,
                user_id = user.Id,
                is_admin = user.Id == session.Client.Id ? 1 : 0
            });
        }

        var channel = await ChatServer.Instance.AddChannel(id);

        data.Channel = new()
        {
            Id = id,
            Name = channelName,
            IsSecret = request.IsSecret,
            Key = key
        };

        foreach (var user in users)
        {
            //TODO: admin 추가
            data.Users.Add(new()
            {
                Id = user.Id,
                Name = user.Nickname
            });
        }

        packet.Encode(data);
        channel.Broadcast(packet);
    }
}