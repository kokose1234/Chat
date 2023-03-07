using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using ProtoChat.Common.Packet.Data.Server;

namespace Chat.Client.Net;

internal sealed class ChatClient : TcpClient
{
    internal static readonly Lazy<ChatClient> Lazy = new(() => new ChatClient("127.0.0.1", 9000));
    internal static ChatClient Instance => Lazy.Value;

    private byte[] _sendKey = null!;
    private byte[] _recvKey = null!;
    private bool _initialized;

    internal ChatClient(string address, int port) : base(address, port)
    {
        ConnectAsync();
    }

    internal void DisconnectAndStop()
    {
        DisconnectAsync();
        while (IsConnected)
            Thread.Yield();

        Environment.Exit(0);
    }

    protected override void OnConnected() => Console.WriteLine($"ChatServer와 연결 완료. Id = {Id}");

    protected override void OnDisconnected()
    {
        Console.WriteLine($"ChatServer와 연결이 끊어짐");
        DisconnectAndStop();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        if (size >= 8)
        {
            Array.Resize(ref buffer, (int)size);
            if (_initialized)
            {
                using var packet = new InPacket(Decrypt(buffer));
                var headerName = Enum.GetName(typeof(ServerHeader), packet.Header) ?? string.Format($"0x{packet.Header:X4}");
                Console.WriteLine($"[S->C] [{headerName}]\r\n{packet}");

                if (PacketHandlers.GetHandler(packet.Header, out var handler))
                {
                    if (handler != null)
                    {
                        handler.Handle(this, packet);
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
                using var packet = new InPacket(buffer);

                if (packet.Header == (uint)ServerHeader.ServerHandshake)
                {
                    var handshake = packet.Decode<ServerHandshake>();
                    if (handshake.Version != Constants.Version)
                    {
                        Console.WriteLine("서버와 클라이언트 버전이 일치하지 않음.");
                        DisconnectAndStop();
                        return;
                    }

                    _sendKey = BitConverter.GetBytes(handshake.SendIv);
                    _recvKey = BitConverter.GetBytes(handshake.RecieveIv);
                    _initialized = true;
                }
                else
                {
                    Console.WriteLine("알 수 없는 핸드셰이크 패킷을 수신함.");
                }
            }
        }
        else
        {
            Console.WriteLine("패킷 사이즈가 너무 작음.");
        }
    }

    internal void Send(OutPacket buffer)
    {
        var headerName = Enum.GetName(typeof(ServerHeader), buffer.Header) ?? string.Format($"0x{buffer.Header:X4}");

        buffer.WriteLength();
        base.SendAsync(Encrypt(buffer.Buffer));
        Console.WriteLine($"[C->S] [{headerName}]\r\n{buffer}");
        buffer.Dispose();
    }

    private byte[] Encrypt(byte[] buffer)
    {
        var result = new byte[buffer.Length];

        for (var i = 0; i < buffer.Length; i++)
        {
            result[i] = (byte)(buffer[i] ^ _sendKey[i % 8]);
        }

        return result;
    }

    private byte[] Decrypt(byte[] buffer)
    {
        var result = new byte[buffer.Length];

        for (var i = 0; i < buffer.Length; i++)
        {
            result[i] = (byte)(buffer[i] ^ _recvKey[i % 8]);
        }

        return result;
    }
}