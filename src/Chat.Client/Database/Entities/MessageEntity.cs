using System;

namespace Chat.Client.Database.Entities;

public class MessageEntity : EntityBase
{
    public uint MessageId { get; set; }
    public uint ChannelId { get; set; }
    public uint UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool HasAttachment { get; set; }
}