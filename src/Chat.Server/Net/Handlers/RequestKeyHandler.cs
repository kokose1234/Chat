using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientRequestKey)]
public class RequestKeyHandler : AbstractHandler
{
    internal override Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientRequestKey>();
        var channel = ChatServer.Instance.GetChannel(request.Channel);
        var user = channel?.GetUser(session.Client.Id);
        if (user == null) return Task.CompletedTask;

        user.KeyRequested = true;

        using var packet = new OutPacket(ServerHeader.ServerRequestKey);
        packet.Encode(new ServerRequestKey {Channel = request.Channel});
        channel?.Broadcast(packet, session.Client.Id);

        return Task.CompletedTask;
    }
}