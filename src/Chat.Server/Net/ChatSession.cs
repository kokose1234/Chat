using System.Collections.Concurrent;
using System.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;
using Chat.Common.Tools;
using FastEnumUtility;
using NetCoreServer;
using Nito.AsyncEx;

namespace Chat.Server.Net;

public class ChatSession : TcpSession
{
    public ChatClient Client { get; private set; } = null!;

    private string _remoteEndpoint = "";
    private byte[] _sendKey = new byte[8];
    private byte[] _recvKey = new byte[8];

    private uint _lastPacketSize;
    private readonly List<ArraySegment<byte>> _incompletePackets = new();
    private readonly ConcurrentQueue<ArraySegment<byte>> _rawRecvQueue = new();

    private readonly AsyncLock _lock = new();
    private readonly AsyncConditionVariable _recvCondition;


    public ChatSession(TcpServer server) : base(server)
    {
        _recvCondition = new(_lock);
        var t = new Thread(ReceiveWorker);
        t.Start();
    }

    protected override void OnConnected()
    {
        _sendKey = Randomizer.NextBytes(8);
        _recvKey = Randomizer.NextBytes(8);
        _remoteEndpoint = (Socket.RemoteEndPoint as IPEndPoint)?.Address.ToString()!;
        Client = new ChatClient(this);

        using var packet = new OutPacket((uint) ServerHeader.ServerHandshake);
        packet.Encode(GetHandshake());

        var data = new byte[packet.Length + 4];
        Array.Copy(BitConverter.GetBytes(packet.Length), 0, data, 0, 4);
        Array.Copy(packet.Buffer, 0, data, 4, packet.Length);

        base.SendAsync(data);

        ChatServer.Instance.Clients.TryAdd(Id.ToString(), Client);
        Console.WriteLine($"{_remoteEndpoint}가 연결됨");
    }

    protected override void OnDisconnected()
    {
        ChatServer.Instance.Clients.TryRemove(Id.ToString(), out _);
        ChatServer.Instance.RemoveClientFromChannel(Client);
        Client.OnDisconnected();

        Console.WriteLine($"{_remoteEndpoint}가 연결 해제됨");
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        Array.Resize(ref buffer, (int) size);

        if (size < 4)
        {
            Console.WriteLine("패킷 사이즈가 너무 작음.");
            return;
        }

        var span = new ReadOnlySpan<byte>(buffer, (int) offset, (int) size);
        for (var index = 0; index < size;)
        {
            if (_incompletePackets.Count == 0)
            {
                var packetSize = BitConverter.ToInt32(span.Slice(index, 4)) + 4;
                if (packetSize > size - index)
                {
                    _lastPacketSize = (uint) packetSize;
                    _incompletePackets.Add(new(buffer, (int) offset + index, (int) size - index));
                    return;
                }

                _rawRecvQueue.Enqueue(new ArraySegment<byte>(buffer, (int) offset + index, packetSize));
                _recvCondition.Notify();
                index += packetSize;
            }
            else
            {
                var totalReceivedBytes = _incompletePackets.Sum(x => x.Count);
                var remainingBytes = _lastPacketSize - totalReceivedBytes;

                if (remainingBytes > size - index)
                {
                    _incompletePackets.Add(new(buffer, (int) offset + index, (int) size - index));
                    return;
                }

                _incompletePackets.Add(new(buffer, (int) offset + index, (int) remainingBytes));

                var dataLength = _incompletePackets.Sum(x => x.Count);
                var data = new byte[dataLength];
                var dataOffset = 0;

                foreach (var segment in _incompletePackets)
                {
                    System.Buffer.BlockCopy(segment.Array, segment.Offset, data, dataOffset, segment.Count);
                    dataOffset += segment.Count;
                }

                // var data = _incompletePackets
                //            .SelectMany(segment => segment.Array[segment.Offset..(segment.Offset + segment.Count)])
                //            .ToArray();

                _rawRecvQueue.Enqueue(new(data));
                _recvCondition.Notify();
                _incompletePackets.Clear();
                index += (int) remainingBytes;
            }
        }
    }

    internal void Close()
    {
        Disconnect();
        while (IsConnected)
            Thread.Yield();
    }

    internal void Send(OutPacket buffer, bool dispose = true)
    {
        var data = new byte[buffer.Length + 4];
        var encrypted = Encrypt(buffer.Buffer);

        Array.Copy(BitConverter.GetBytes(buffer.Length), 0, data, 0, 4);
        Array.Copy(encrypted, 0, data, 4, encrypted.Length);

        base.SendAsync(data);
        if (dispose) buffer.Dispose();

        if (data.Length <= 256)
        {
            var headerName = FastEnum.GetName((ServerHeader) buffer.Header) ?? string.Format($"0x{buffer.Header:X4}");
            Console.WriteLine($"[S->C] [{headerName}]\r\n{buffer}");
        }
    }

    private byte[] Encrypt(byte[] buffer)
    {
#if DEBUG
        return buffer;
#endif
        var result = new byte[buffer.Length];

        for (var i = 0; i < buffer.Length; i++)
        {
            result[i] = (byte) (buffer[i] ^ _recvKey[i % 8]);
        }

        return result;
    }

    private byte[] Decrypt(byte[] buffer)
    {
#if DEBUG
        return buffer;
#endif
        var result = new byte[buffer.Length];

        for (var i = 0; i < buffer.Length; i++)
        {
            result[i] = (byte) (buffer[i] ^ _sendKey[i % 8]);
        }

        return result;
    }

    private ServerHandshake GetHandshake()
    {
        return new()
        {
            Version = Constants.VERSION,
            SendIv = BitConverter.ToUInt64(_sendKey),
            RecieveIv = BitConverter.ToUInt64(_recvKey)
        };
    }

    private async void ReceiveWorker()
    {
        while (true)
        {
            await _recvCondition.WaitAsync();

            while (_rawRecvQueue.TryDequeue(out var buffer))
            {
                var length = BitConverter.ToUInt32(buffer.Array!, buffer.Offset);
                if (length < 4)
                {
                    Console.WriteLine("패킷 사이즈가 너무 작음.");
                    return;
                }

                var data = new byte[length];
                Array.Copy(buffer.Array!, buffer.Offset + 4, data, 0, length);

                using var packet = new InPacket(Decrypt(data), length);

                if (data.Length <= 1024)
                {
                    var headerName = FastEnum.GetName((ClientHeader) packet.Header) ?? string.Format($"0x{packet.Header:X4}");
                    Console.WriteLine($"[C->S] [{headerName}]\r\n{packet}");
                }

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
        }
    }
}