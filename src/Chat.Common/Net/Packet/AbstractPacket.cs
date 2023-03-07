using Chat.Common.Tools;

namespace Chat.Common.Net.Packet;

public abstract class AbstractPacket : IDisposable
{
    protected MemoryStream stream = null!;

    public int Length => (int)stream.Length;
    public byte[] Buffer => stream.ToArray();
    public int Available => Length - Position;

    public int Position
    {
        get => (int)stream.Position;
        set => stream.Position = value;
    }

    public override string ToString() => PrettyHexDump.HexDump(Buffer);

    public void Dispose() => stream.Dispose();
}