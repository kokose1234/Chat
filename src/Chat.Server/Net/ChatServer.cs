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
    private readonly ConcurrentBag<User> _users = new();

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

    public ChatClient GetClient(string id) => Clients[id];

    public IEnumerable<Channel> GetChannels(uint userId) => _channels.Where(x => x.Users.Any(y => y.Id == userId)).ToImmutableList();

    public Channel? GetChannel(uint channelId) => _channels.FirstOrDefault(x => x.Id == channelId, null);

    public List<Channel> GetAllChannels() => _channels.ToList();

    public User? GetUser(uint userId) => _users.FirstOrDefault(x => x.Id == userId, null);

    public List<User> GetUsers(uint[] ids) => _users.Where(x => ids.Contains(x.Id)).ToList();

    public List<User> GetAllUsers() => _users.ToList();

    public void AddClient(ChatClient client)
    {
        foreach (var channel in _channels)
        {
            var user = channel.GetUser(client.Id);
            if (user == null) continue;

            user.Client = client;
        }

        foreach (var user in _users)
        {
            if (user.Id != client.Id) continue;
            user.Client = client;
            break;
        }
    }

    public void AddUser(User user)
    {
        if (_users.Any(x => x.Id == user.Id)) return;
        _users.Add(user);
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

    public async Task<Channel> AddChannel(uint id)
    {
        using var mutex = await DatabaseManager.Mutex.ReaderLockAsync();
        var channels = await DatabaseManager.Factory.Query("channels").Where("id", id).FirstAsync();
        var channel = new Channel(channels.id, channels.name, channels.is_secret == 1);
        _channels.Add(channel);

        var channelUsers = await DatabaseManager.Factory.Query("channel_users").Where("channel_id", id).GetAsync();
        foreach (var data in channelUsers)
        {
            var user = GetUser((uint) data.user_id);
            if (user == null) continue;
            var channelUser = new ChannelUser(user, data.is_admin == 1)
            {
                Client = user.Client
            };
            channel.Users.Add(channelUser);
        }

        return channel;
    }

    private async void LoadFromDatabase()
    {
        using var mutex = await DatabaseManager.Mutex.ReaderLockAsync();
        using var packet = new OutPacket(ServerHeader.ServerLogin);
        var accounts = await DatabaseManager.Factory.Query("accounts").GetAsync();
        var channels = await DatabaseManager.Factory.Query("channels").GetAsync();

        foreach (var account in accounts)
        {
            var user = new User(account.id, account.username, account.name, account.message, account.avatar_update_date);
            _users.Add(user);
        }

        foreach (var data in channels)
        {
            var channel = new Channel(data.id, data.name, data.is_secret == 1);
            _channels.Add(channel);
        }

        var channelUsers = await DatabaseManager.Factory.Query("channel_users").GetAsync();
        foreach (var data in channelUsers)
        {
            var channel = GetChannel((uint) data.channel_id);
            var user = GetUser((uint) data.user_id);
            if (user == null) continue;
            var channelUser = new ChannelUser(user, data.is_admin == 1);
            channel?.Users.Add(channelUser);
        }
    }
}