using Chat.Common.Net.Packet.Header;
using ProtoBuf;

namespace Chat.Common.Net.Packet;

public class OutPacket : AbstractPacket
{
    public uint Header { get; }
    public bool Disposed { get; }

    public OutPacket(int size = 64)
    {
        stream = new MemoryStream(size);
        Disposed = false;
        stream.Seek(4, SeekOrigin.Begin); // skip 4 bytes for packet length
    }

    public OutPacket(uint header) : this()
    {
        Header = header;
        stream.Write(BitConverter.GetBytes(header), 0, 4);
    }

    public OutPacket(ServerHeader header) : this((uint)header) { }
    public OutPacket(ClientHeader header) : this((uint)header) { }

    public void Encode<T>(T data)
    {
        ThrowIfDisposed();
        Serializer.Serialize(stream, data);
    }

    public void WriteLength()
    {
        stream.Position = 0;
        stream.Write(BitConverter.GetBytes(Length - 4), 0, 4);
    }

    private void ThrowIfDisposed()
    {
        if (Disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}