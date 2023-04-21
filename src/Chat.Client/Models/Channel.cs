using System;

namespace Chat.Client.Models;

public sealed record Channel
{
    public uint Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSecret { get; set; }
    public byte[] Key { get; set; } = Array.Empty<byte>();
}