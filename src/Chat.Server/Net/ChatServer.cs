using System.Collections.Concurrent;
using Chat.Server.Data;
using NetCoreServer;

namespace Chat.Server.Net;

internal class ChatServer : TcpServer
{
    internal static ChatServer Instance { get; private set; } = null!;
    internal static ConcurrentDictionary<string, ChatClient> Clients { get; } = new();

    public ChatServer(string address, int port) : base(address, port)
    {
        Instance = this;
    }

    protected override TcpSession CreateSession() => new ChatSession(this);
}