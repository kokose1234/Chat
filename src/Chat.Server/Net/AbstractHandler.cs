using Chat.Common.Net.Packet;

namespace Chat.Server.Net;

internal abstract class AbstractHandler
{
    internal abstract void Handle(ChatSession session, InPacket packet);
}