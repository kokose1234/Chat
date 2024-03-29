﻿using System;

namespace Chat.Client.Database.Entities;

public sealed class ChannelEntity : EntityBase
{
    public uint ChannelId { get; set; }
    public uint LastMessageId { get; set; }
    public bool IsSecret { get; set; }
    public byte[] Key { get; set; } = Array.Empty<byte>();
}