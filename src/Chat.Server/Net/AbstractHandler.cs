using Chat.Common.Net.Packet;

namespace Chat.Server.Net;

public abstract class AbstractHandler
{
    internal abstract void Handle(ChatSession session, InPacket packet);
}