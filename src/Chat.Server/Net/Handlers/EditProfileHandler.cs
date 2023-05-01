using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientEditProfile)]
public class EditProfileHandler : AbstractHandler
{
    internal override Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientEditProfile>();
        var user = ChatServer.Instance.GetUser(session.Client.Id);
        if (user == null) return Task.CompletedTask;
        var channelUsers = ChatServer.Instance.GetChannelUsers(session.Client.Id);

        user.Nickname = request.Name;
        user.Message = request.Message;
        if (request.Picture != null) user.LastAvatarUpdate = (ulong) DateTime.Now.Ticks;

        foreach (var channelUser in channelUsers)
        {
            channelUser.Nickname = request.Name;
            channelUser.Message = request.Message;
            if (request.Picture != null) channelUser.LastAvatarUpdate = user.LastAvatarUpdate;
        }

        if (request.Picture != null)
        {
            DatabaseManager.Factory.Query("accounts").Where("id", session.Client.Id).Update(new
            {
                name = request.Name,
                message = request.Message,
                avatar = request.Picture,
                avatar_update_date = user.LastAvatarUpdate
            });
        }
        else
        {
            DatabaseManager.Factory.Query("accounts").Where("id", session.Client.Id).Update(new
            {
                name = request.Name,
                message = request.Message
            });
        }

        return Task.CompletedTask;
    }
}