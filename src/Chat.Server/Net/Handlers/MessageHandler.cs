using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientMessage)]
public class MessageHandler : AbstractHandler
{
    internal override Task Handle(ChatSession session, InPacket inPacket)
    {
        var data = inPacket.Decode<ClientMessage>();
        var channel = ChatServer.Instance.GetChannel(data.Channel);
        var id = DatabaseManager.Factory.Query("messages").InsertGetId<uint>(new
        {
            channel_id = data.Channel,
            user_id = session.Client.Id,
            date = DateTime.Now,
            message = data.Attachment == null ? data.Message : null,
            attachment = data.Attachment
        });

        channel?.OnMessage(id, session.Client, data);

        return Task.CompletedTask;
    }
}