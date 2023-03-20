using Chat.Common.Net.Packet;

namespace Chat.Server.Net;

public abstract class AbstractHandler
{
    internal abstract Task Handle(ChatSession session, InPacket inPacket);
}