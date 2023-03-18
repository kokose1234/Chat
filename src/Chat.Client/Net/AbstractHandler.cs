using Chat.Common.Net.Packet;

namespace Chat.Client.Net;

public abstract class AbstractHandler
{
    internal abstract void Handle(ChatClient session, InPacket packet);
}