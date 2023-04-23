using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientResponseKey)]
public class KeyResponseHandler : AbstractHandler
{
    internal override Task Handle(ChatSession session, InPacket inPacket)
    {
        var response = inPacket.Decode<ClientResponseKey>();
        var channel = ChatServer.Instance.GetChannel(response.Channel);
        var user = channel?.Users.FirstOrDefault(x => x is {KeyRequested: true, Client: not null}, null);
        if (user == null) return Task.CompletedTask;

        using var packet = new OutPacket(ServerHeader.ServerDeliverKey);
        var data = new ServerDeliverKey {Channel = response.Channel, Key = response.Key};
        packet.Encode(data);
        user.Send(packet);
        user.KeyRequested = false;

        return Task.CompletedTask;
    }
}