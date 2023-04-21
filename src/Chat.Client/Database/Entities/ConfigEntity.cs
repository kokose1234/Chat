namespace Chat.Client.Database.Entities;

public sealed class ConfigEntity : EntityBase
{
    public uint LastMessage { get; set; }
}