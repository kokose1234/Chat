using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientEditProfile)]
public class EditProfileHandler : AbstractHandler
{
    internal override Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientEditProfile>();
        
        return Task.CompletedTask;
    }
}