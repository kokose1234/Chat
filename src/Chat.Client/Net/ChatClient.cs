using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chat.Client.ViewModels;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;
using FastEnumUtility;
using NetCoreServer;
using Nito.AsyncEx;

namespace Chat.Client.Net;

internal sealed class ChatClient : TcpClient
{
    internal static readonly Lazy<ChatClient> Lazy = new(() => new ChatClient("127.0.0.1", 9000));
    internal static ChatClient Instance => Lazy.Value;

    public int UserId { get; set; }
    public MainWindowViewModel ViewModel { get; set; }

    private byte[] _sendKey = null!;
    private byte[] _recvKey = null!;
    private bool _initialized;

    private uint _lastPacketSize = 0;
    private readonly List<ArraySegment<byte>> _incompletePackets = new();
    private readonly ConcurrentQueue<ArraySegment<byte>> _rawRecvQueue = new();

    private readonly AsyncLock _lock = new();
    private readonly AsyncConditionVariable _recvCondition;

    internal ChatClient(string address, int port) : base(address, port)
    {
        OptionReceiveBufferSize = 65536;
        OptionNoDelay = true;
        OptionKeepAlive = true;

        _recvCondition = new(_lock);
        var t = new Thread(ReceiveWorker);
        t.Start();

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
        Console.WriteLine("ChatServer와 연결이 끊어짐");
        DisconnectAndStop();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
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

                _rawRecvQueue.Enqueue(new ArraySegment<byte>(buffer, (int) offset + index, packetSize + 4));
                _recvCondition.Notify();
                index += packetSize + 4;
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

                var data = _incompletePackets
                           .SelectMany(segment => segment.Array[segment.Offset..(segment.Offset + segment.Count)])
                           .ToArray();

                _rawRecvQueue.Enqueue(new(data));
                _recvCondition.Notify();
                _incompletePackets.Clear();
                index += (int) remainingBytes;
            }
        }
    }

    internal void Send(OutPacket buffer)
    {
        var data = new byte[buffer.Length + 4];
        var encrypted = Encrypt(buffer.Buffer);

        Array.Copy(BitConverter.GetBytes(buffer.Length), 0, data, 0, 4);
        Array.Copy(encrypted, 0, data, 4, encrypted.Length);

        base.SendAsync(data);
        buffer.Dispose();

        if (data.Length < 1024)
        {
            var headerName = FastEnum.GetName((ClientHeader) buffer.Header) ?? string.Format($"0x{buffer.Header:X4}");
            Console.WriteLine($"[C->S] [{headerName}]\r\n{buffer}");
        }
    }

    private byte[] Encrypt(byte[] buffer)
    {
        var result = new byte[buffer.Length];

        for (var i = 0; i < buffer.Length; i++)
        {
            result[i] = (byte) (buffer[i] ^ _sendKey[i % 8]);
        }

        return result;
    }

    private byte[] Decrypt(byte[] buffer)
    {
        var result = new byte[buffer.Length];

        for (var i = 0; i < buffer.Length; i++)
        {
            result[i] = (byte) (buffer[i] ^ _recvKey[i % 8]);
        }

        return result;
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

                if (_initialized)
                {
                    using var packet = new InPacket(Decrypt(data), length);

                    if (length <= 1024)
                    {
                        var headerName = FastEnum.GetName((ServerHeader) packet.Header) ?? string.Format($"0x{packet.Header:X4}");
                        Console.WriteLine($"[S->C] [{headerName}]\r\n{packet}");
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
                else
                {
                    using var packet = new InPacket(data, length);

                    if (packet.Header == (uint) ServerHeader.ServerHandshake)
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
        }
    }
}