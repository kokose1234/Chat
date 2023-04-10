using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Server.Data;
using Chat.Server.Database;
using NetCoreServer;
using SqlKata.Execution;

namespace Chat.Server.Net;

internal class ChatServer : TcpServer
{
    internal static ChatServer Instance { get; private set; } = null!;

    internal ConcurrentDictionary<string, ChatClient> Clients { get; } = new();

    private readonly ConcurrentBag<Channel> _channels = new();

    public ChatServer(IPAddress address, int port) : base(address, port)
    {
        Instance = this;
        OptionReceiveBufferSize = ushort.MaxValue;
        OptionSendBufferSize = ushort.MaxValue;
        OptionNoDelay = true;
        OptionKeepAlive = true;

        DatabaseManager.Setup();
        LoadFromDatabase();
    }


    protected override TcpSession CreateSession() => new ChatSession(this);

    public IEnumerable<Channel> GetChannels(uint userId) => _channels.Where(x => x.Users.Any(y => y.Id == userId)).ToImmutableList();

    public Channel? GetChannel(uint channelId) => _channels.FirstOrDefault(x => x.Id == channelId, null);

    public void AddClientToChannel(ChatClient client)
    {
        foreach (var channel in _channels)
        {
            var user = channel.GetUser(client.Id);
            if (user == null) continue;

            user.Client = client;
        }
    }

    public void RemoveClientFromChannel(ChatClient? client)
    {
        if (client == null) return;

        foreach (var channel in _channels)
        {
            var user = channel.GetUser(client.Id);
            if (user == null) continue;

            user.Client = null;
        }
    }

    private async void LoadFromDatabase()
    {
        using var mutex = await DatabaseManager.Mutex.ReaderLockAsync();
        using var packet = new OutPacket(ServerHeader.ServerLogin);
        var channels = await DatabaseManager.Factory.Query("channels").GetAsync();

        foreach (var data in channels)
        {
            var channel = new Channel(data.id, data.name, data.is_secret == 1);
            _channels.Add(channel);
        }

        var channelUsers = await DatabaseManager.Factory.Query("channel_users").GetAsync();
        foreach (var user in channelUsers)
        {
            var channel = GetChannel((uint) user.channel_id);
            channel?.Users.Add(new ChannelUser((uint) user.user_id, user.is_admin == 1));
        }
    }
}