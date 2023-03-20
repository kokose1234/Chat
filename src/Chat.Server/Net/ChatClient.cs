using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;
using Chat.Common.Tools;

namespace Chat.Server.Net;

internal class ChatClient
{
    public long LastPongTime { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    public int LastPingKey { get; set; }

    private readonly Timer _pingTimer;
    private readonly ChatSession _session;

    internal ChatClient(ChatSession session)
    {
        _pingTimer = new Timer(OnPingTimer);
        _pingTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1));
        _session = session;
    }

    private void OnPingTimer(object? state)
    {
        if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - LastPongTime > 120000)
        {
            _session.Close();
            return;
        }

        using var packet = new OutPacket(ServerHeader.ServerPing);
        var ping = new ServerPing {Key = Randomizer.NextInt()};
        LastPingKey = ping.Key;

        packet.Encode(ping);
        _session.Send(packet);

        Console.WriteLine($"[S] Ping: {ping.Key}");
    }
}