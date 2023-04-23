namespace Chat.Client.Data;

public record UserSearchResult
{
    public uint Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Nickname { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
#if DEBUG
    public byte[]? Avatar { get; set; } = null!;
#else
    public byte[]? Avatar { get; init; } = null!;
#endif
}