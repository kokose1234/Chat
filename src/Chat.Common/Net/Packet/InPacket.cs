using ProtoBuf;

namespace Chat.Common.Net.Packet;

public class InPacket : AbstractPacket
{
    public uint PacketLen { get; }
    public uint Header { get; }

    public InPacket(byte[] buffer, bool readSize = true, bool readHeader = true)
    {
        stream = new MemoryStream(buffer, false);

        if (readSize)
        {
            PacketLen = BitConverter.ToUInt32(buffer, Position);
            stream.Seek(4, SeekOrigin.Current);
        }

        if (readHeader)
        {
            Header = BitConverter.ToUInt32(buffer, Position);
            stream.Seek(4, SeekOrigin.Current);
        }
    }

    public void Skip(int count) => stream.Seek(count, SeekOrigin.Current);

    public T Decode<T>() => Serializer.Deserialize<T>(stream);
}