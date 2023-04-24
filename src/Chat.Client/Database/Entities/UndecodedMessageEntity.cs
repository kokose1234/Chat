using System;

namespace Chat.Client.Database.Entities;

public class UndecodedMessageEntity : EntityBase
{
    public uint MessageId { get; set; }
    public uint ChannelId { get; set; }
    public uint UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public byte[] Content { get; set; } = null!;
    public bool HasAttachment { get; set; }
}