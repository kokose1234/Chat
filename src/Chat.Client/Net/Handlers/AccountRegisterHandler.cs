using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerAccountRegister)]
internal class AccountRegisterHandler : AbstractHandler
{
    internal override void Handle(ChatClient session, InPacket packet)
    {
        
        
    }
}