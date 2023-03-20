using System.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;
using Chat.Common.Tools;
using Chat.Server.Data;
using FastEnumUtility;
using NetCoreServer;

namespace Chat.Server.Net;

internal class ChatSession : TcpSession
{
    internal ChatClient Client { get; private set; } = null!;

    private string _remoteEndpoint = "";
    private byte[] _sendKey = new byte[8];
    private byte[] _recvKey = new byte[8];

    public ChatSession(TcpServer server) : base(server) { }

    protected override void OnConnected()
    {
        _sendKey = Randomizer.NextBytes(8);
        _recvKey = Randomizer.NextBytes(8);
        _remoteEndpoint = (Socket.RemoteEndPoint as IPEndPoint)?.Address.ToString()!;
        Client = new ChatClient(this);

        using var packet = new OutPacket((uint)ServerHeader.ServerHandshake);
        packet.Encode(GetHandshake());
        packet.WriteLength();

        base.SendAsync(packet.Buffer);

        ChatServer.Clients.TryAdd(Id.ToString(), Client);
        Console.WriteLine($"{_remoteEndpoint}가 연결됨");
    }

    protected override void OnDisconnected()
    {
        ChatServer.Clients.TryRemove(Id.ToString(), out _);
        Console.WriteLine($"{_remoteEndpoint}가 연결 해제됨");
    }

    protected override async void OnReceived(byte[] buffer, long offset, long size)
    {
        if (size >= 8)
        {
            Array.Resize(ref buffer, (int)size);
            using var packet = new InPacket(Decrypt(buffer));
            var headerName = FastEnum.GetName((ClientHeader) packet.Header) ?? string.Format($"0x{packet.Header:X4}");
            Console.WriteLine($"[C->S] [{headerName}]\r\n{packet}");

            if (PacketHandlers.GetHandler(packet.Header, out var handler))
            {
                if (handler != null)
                {
                    await handler.Handle(this, packet);
                }
                else
                {
                    Console.WriteLine($"패킷 Id {packet.Header}의 패킷 핸들러가 존재하지 않음.");
                }
            }
            else
            {
                Console.WriteLine("알 수 없는 패킷을 수신함.");
            }
        }
        else
        {
            Console.WriteLine("패킷 사이즈가 너무 작음.");
        }
    }

    internal void Send(OutPacket buffer)
    {
        var headerName = FastEnum.GetName((ServerHeader) buffer.Header) ?? string.Format($"0x{buffer.Header:X4}");

        buffer.WriteLength();
        base.SendAsync(Encrypt(buffer.Buffer));
        Console.WriteLine($"[S->C] [{headerName}]\r\n{buffer}");
        buffer.Dispose();
    }

    private byte[] Encrypt(byte[] buffer)
    {
        var result = new byte[buffer.Length];

        for (var i = 0; i < buffer.Length; i++)
        {
            result[i] = (byte)(buffer[i] ^ _recvKey[i % 8]);
        }

        return result;
    }

    private byte[] Decrypt(byte[] buffer)
    {
        var result = new byte[buffer.Length];

        for (var i = 0; i < buffer.Length; i++)
        {
            result[i] = (byte)(buffer[i] ^ _sendKey[i % 8]);
        }

        return result;
    }

    private ServerHandshake GetHandshake()
    {
        return new()
        {
            Version = Constants.Version,
            SendIv = BitConverter.ToUInt64(_sendKey),
            RecieveIv = BitConverter.ToUInt64(_recvKey)
        };
    }
}