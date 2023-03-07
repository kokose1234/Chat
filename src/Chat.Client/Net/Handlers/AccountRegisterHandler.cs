using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using ProtoChat.Common.Packet.Data.Server;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerAccountRegister)]
internal class AccountRegisterHandler : AbstractHandler
{
    internal override void Handle(ChatClient session, InPacket packet)
    {
        var response = packet.Decode<ServerAccountRegister>();
        
    }
}