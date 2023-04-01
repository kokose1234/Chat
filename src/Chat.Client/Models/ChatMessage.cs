using System;
using Chat.Common.Data;

namespace Chat.Client.Models;

public class ChatMessage : Message
{
    public uint Id { get; set; }
    public uint ChannelId { get; set; }
    public uint SenderId { get; set; }
    public string SenderName { get; set; }
    public string Message { get; set; }
    public DateTime Time { get; set; }
}