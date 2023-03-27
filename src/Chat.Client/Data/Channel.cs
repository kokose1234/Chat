namespace Chat.Client.Data;

public sealed record Channel
{
    public uint Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}