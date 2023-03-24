using ProtoBuf;

namespace Chat.Common.Net.Packet;

public class InPacket : AbstractPacket
{
    public uint PacketLen { get; }
    public uint Header { get; }

    public InPacket(byte[] buffer, uint size, bool readHeader = true)
    {
        stream = new MemoryStream(buffer, false);
        PacketLen = size;

        if (readHeader)
        {
            Header = BitConverter.ToUInt32(buffer, Position);
            stream.Seek(4, SeekOrigin.Current);
        }
    }

    public void Skip(int count) => stream.Seek(count, SeekOrigin.Current);

    public T Decode<T>() => Serializer.Deserialize<T>(stream);
}