using Chat.Common.Net.Packet;
using Chat.Server.Net;

namespace Chat.Server.Data;

public class User : ISavableObject
{
    public uint Id { get; }

    public string Username { get; }

    public string Nickname { get; set; }

    public string? Message { get; set; }

    public ulong LastAvatarUpdate { get; set; }

    public ChatClient? Client { get; set; }

    public User(uint id, string username, string nickname, string? message, ulong lastAvatarUpdate)
    {
        Id = id;
        Username = username;
        Nickname = nickname;
        Message = message;
        LastAvatarUpdate = lastAvatarUpdate;
    }

    protected User(User other)
    {
        Id = other.Id;
        Username = other.Username;
        Nickname = other.Nickname;
        Message = other.Message;
        LastAvatarUpdate = other.LastAvatarUpdate;
    }

    public void Send(OutPacket packet, bool dispose = true)
    {
        Client?.Session.Send(packet, dispose);
    }


    public void Save() { }

    public virtual void OnDisconnected()
    {
        Client = null;
    }
}