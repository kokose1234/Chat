using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientStartChat)]
public class StartChatHandler : AbstractHandler
{
    internal override async Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientStartChat>();
        var user = ChatServer.Instance.GetUser(request.Id);
        if (user == null) return;
        if (ChatServer.Instance.GetChannels(session.Client.Id).Any(x => x.Users.Any(x => x.Id == request.Id))) return;

        using var packet = new OutPacket(ServerHeader.ServerCreateChannel);
        var data = new ServerCreateChannel();
        bool isFriend;
        uint id;

        using (var mutex = await DatabaseManager.Mutex.WriterLockAsync())
        {
            isFriend = (await DatabaseManager.Factory.Query("friends").Where(new {user_id = session.Client.Id, friend_user_id = request.Id}).CountAsync<int>()) == 1;
            id = await DatabaseManager.Factory.Query("channels").InsertGetIdAsync<uint>(new
            {
                name = $"{session.Client.Name}, {user.Nickname}",
                is_secret = 0
            });
            await DatabaseManager.Factory.Query("channel_users").InsertAsync(new
            {
                channel_id = id,
                user_id = session.Client.Id
            });
            await DatabaseManager.Factory.Query("channel_users").InsertAsync(new
            {
                channel_id = id,
                user_id = request.Id
            });
        }

        var channel = await ChatServer.Instance.AddChannel(id);

        data.Channel = new()
        {
            Id = (uint) id,
            Name = $"{session.Client.Name}, {user.Nickname}",
            IsSecret = false,
        };

        data.Users.Add(new()
        {
            Id = session.Client.Id,
            Name = session.Client.Name,
        });
        data.Users.Add(new()
        {
            Id = user.Id,
            Name = user.Nickname,
        });

        packet.Encode(data);
        channel.Broadcast(packet);
    }
}