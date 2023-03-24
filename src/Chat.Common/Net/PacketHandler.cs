using Chat.Common.Net.Packet.Header;

namespace Chat.Common.Net;

public class PacketHandler : Attribute
{
    public uint Header { get; } = 0;
        
    public PacketHandler(ServerHeader header) => Header = (uint)header;
    public PacketHandler(ClientHeader header) => Header = (uint)header;
}