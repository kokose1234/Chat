using Chat.Common.Net.Packet;
using Chat.Server.Net;

namespace Chat.Server.Data;

public sealed class ChannelUser : ISavableObject
{
    public uint Id { get; }

    public ChatClient? Client { get; set; }

    public bool IsAdmin { get; set; }

    public ChannelUser(uint id, bool isAdmin)
    {
        Id = id;
        IsAdmin = isAdmin;
    }

    public void Send(OutPacket packet, bool dispose = true)
    {
        Client?.Session.Send(packet, dispose);
    }

    public void Save()
    {
        throw new NotImplementedException();
    }
}