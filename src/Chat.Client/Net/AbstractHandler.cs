using System.Threading.Tasks;
using Chat.Common.Net.Packet;

namespace Chat.Client.Net;

public abstract class AbstractHandler
{
    internal abstract Task Handle(ChatClient session, InPacket inPacket);
}