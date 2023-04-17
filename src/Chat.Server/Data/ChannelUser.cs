namespace Chat.Server.Data;

public sealed class ChannelUser : User
{
    public bool IsAdmin { get; set; }

    public ChannelUser(User user, bool isAdmin) : base(user)
    {
        IsAdmin = isAdmin;
    }

    public new void Save()
    {
        throw new NotImplementedException();
    }
}