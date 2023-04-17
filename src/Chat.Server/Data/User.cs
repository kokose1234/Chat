using Chat.Common.Net.Packet;
using Chat.Server.Net;

namespace Chat.Server.Data;

public class User : ISavableObject
{
    public uint Id { get; }

    public string Username { get; }

    public string Nickname { get; set; }

    public string? Message { get; set; }

    public byte[]? Avatar { get; set; }

    public bool HasAvatar => Avatar != null;

    public ChatClient? Client { get; set; }

    public User(uint id, string username, string nickname, string? message, byte[]? avatar)
    {
        Id = id;
        Username = username;
        Nickname = nickname;
        Message = message;
        Avatar = avatar;
    }

    protected User(User other)
    {
        Id = other.Id;
        Username = other.Username;
        Nickname = other.Nickname;
        Message = other.Message;
        Avatar = other.Avatar;
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