using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;
using Chat.Common.Tools;
using Chat.Server.Data;

namespace Chat.Server.Net;

public class ChatClient : ISavableObject
{
    public long LastPongTime { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    public int LastPingKey { get; set; }

    public uint Id { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public bool IsAdmin { get; set; }
    public ChatSession Session { get; }

    private readonly Timer _pingTimer;

    internal ChatClient(ChatSession session)
    {
        _pingTimer = new Timer(OnPingTimer);
        _pingTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1));
        Session = session;
    }

    private void OnPingTimer(object? state)
    {
        if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - LastPongTime > 120000)
        {
            Session.Close();
            return;
        }

        using var packet = new OutPacket(ServerHeader.ServerPing);
        var ping = new ServerPing {Key = Randomizer.NextInt()};
        LastPingKey = ping.Key;

        packet.Encode(ping);
        Session.Send(packet);

        Console.WriteLine($"[S] Ping: {ping.Key}");
    }

    public void Save() { }

    public void OnDisconnected()
    {
        var user = ChatServer.Instance.GetUser(Id);
        var channels = ChatServer.Instance.GetChannels(Id);

        if (user != null)
        {
            user.Save();
            user.OnDisconnected();
        }

        foreach (var channel in channels)
        {
            foreach (var channelUser in channel.Users)
            {
                channelUser.Save();
                channelUser.OnDisconnected();
            }
        }

        _pingTimer.Dispose();
    }
}