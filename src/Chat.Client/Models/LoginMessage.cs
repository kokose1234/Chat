using Chat.Client.Data.Types;

namespace Chat.Client.Models;

public sealed class LoginMessage
{
    public LoginMessageType Type { get; init; }

    public string Message { get; init; } = string.Empty;
}