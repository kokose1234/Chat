using System.Collections.Concurrent;
using System.Net;
using NetCoreServer;

namespace Chat.Server.Net;

internal class ChatServer : TcpServer
{
    internal static ChatServer Instance { get; private set; } = null!;
    internal static ConcurrentDictionary<string, ChatClient> Clients { get; } = new();

    public ChatServer(IPAddress address, int port) : base(address, port)
    {
        Instance = this;
        OptionReceiveBufferSize = 65536;
        OptionNoDelay = true;
        OptionKeepAlive = true;
    }


    protected override TcpSession CreateSession() => new ChatSession(this);
}