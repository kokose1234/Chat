using Chat.Common.Net.Packet.Header;
using ProtoBuf;

namespace Chat.Common.Net.Packet;

public class OutPacket : AbstractPacket
{
    public uint Header { get; }
    public bool Disposed { get; }

    public OutPacket(int size = byte.MaxValue)
    {
        stream = new MemoryStream(size);
        Disposed = false;
    }

    public OutPacket(uint header) : this()
    {
        Header = header;
        stream.Write(BitConverter.GetBytes(header), 0, 4);
    }

    public OutPacket(ServerHeader header) : this((uint) header) { }

    public OutPacket(ClientHeader header) : this((uint) header) { }

    public void Encode<T>(T data) where T : IExtensible
    {
        ThrowIfDisposed();
        Serializer.Serialize(stream, data);
    }

    private void ThrowIfDisposed()
    {
        if (Disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}