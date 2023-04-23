using System.Collections.Concurrent;
using Chat.Common.Data;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Net;

namespace Chat.Server.Data;

public sealed class Channel : ISavableObject
{
    public uint Id { get; }

    public string Name { get; set; }

    public bool IsSecret { get; }

    public ConcurrentBag<ChannelUser> Users { get; } = new();

    public Channel(uint id, string name, bool isSecret)
    {
        Id = id;
        Name = name;
        IsSecret = isSecret;
    }

    public ChannelUser? GetUser(uint id) => Users.FirstOrDefault(x => x.Id == id, null);

    public void OnMessage(ChatClient sender, ClientMessage message)
    {
        var packet = new OutPacket(ServerHeader.ServerMessage);
        var data = new Message
        {
            ChannelId = Id,
            Sender = sender.Id,
            Text = message.Message,
            Attachment = message.Attachment,
            Date = DateTime.Now
        };
        var serverMessage = new ServerMessage {Message = data};

        packet.Encode(serverMessage);
        Broadcast(packet);
    }

    public void Broadcast(OutPacket packet)
    {
        foreach (var user in Users)
        {
            user.Send(packet, false);
        }

        packet.Dispose();
    }

    public void Broadcast(OutPacket packet, ChannelUser except)
    {
        foreach (var user in Users)
        {
            if (user == except) continue;
            user.Send(packet, false);
        }

        packet.Dispose();
    }

    public void Broadcast(OutPacket packet, uint except)
    {
        foreach (var user in Users)
        {
            if (user.Id == except) continue;
            user.Send(packet, false);
        }

        packet.Dispose();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }
}