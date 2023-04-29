using System.Linq;
using System.Threading.Tasks;
using Chat.Client.Database;
using Chat.Client.Database.Repositories;
using Chat.Client.Models;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using DynamicData;
using LiteDB;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerChannelSync)]
public class ChannelSyncHandler : AbstractHandler
{
    internal override async Task Handle(ChatClient session, InPacket inPacket)
    {
        var data = inPacket.Decode<ServerChannelSync>();
        var repo = DatabaseManager.GetRepository<ChannelRepository>();
        var messageRepo = DatabaseManager.GetRepository<MessageRepository>();

        session.ViewModel.Channels.Clear();

        foreach (var channel in data.Channels)
        {
            var result = repo.GetChannel(channel.Id);
            var info = new Channel
            {
                Id = channel.Id,
                Name = channel.Name,
                Description = string.Empty,
                IsSecret = channel.IsSecret
            };

            if (result == null)
            {
                result = new()
                {
                    Id = ObjectId.NewObjectId(),
                    ChannelId = channel.Id,
                    IsSecret = channel.IsSecret
                };
                repo.AddChannel(result);
                RequestKey(channel.Id, session);
            }
            else
            {
                if (channel.IsSecret)
                {
                    if (result.Key.Length != 0)
                    {
                        info.Key = result.Key;
                    }
                    else
                    {
                        RequestKey(channel.Id, session);
                    }
                }
            }

            session.ViewModel.Channels.Add(info);

            var messages = messageRepo.GetMessages(channel.Id);

            foreach (var messageEntity in messages)
            {
                var chatMessage = new ChatMessage
                {
                    Id = messageEntity.MessageId,
                    ChannelId = messageEntity.ChannelId,
                    SenderId = messageEntity.UserId,
                    SenderName = session.ViewModel.Users.FirstOrDefault(x => x.Id == messageEntity.UserId)?.Name ?? "(null)",
                    Message = messageEntity.Content,
                    Time = messageEntity.Timestamp
                };

                if (messageEntity.HasAttachment)
                {
                    session.ViewModel.AddImageMessage(chatMessage);
                    continue;
                }

                session.ViewModel.AddMessage(chatMessage);
            }
        }

        using var packet = new OutPacket(ClientHeader.ClientMessageSync);
        var request = new ClientMessageSync();
        var channels = repo.GetAllChannels();
        var ids = channels?.Where(x => x is not {IsSecret: true, Key.Length: 0}).ToDictionary(x => x.ChannelId, x => x.LastMessageId);
        if (ids == null) return;

        foreach (var kvp in ids)
        {
            request.LastMessageIds.Add(kvp.Key, kvp.Value);
        }

        packet.Encode(request);
        session.Send(packet);
    }

    private void RequestKey(uint id, ChatClient client)
    {
        using var packet = new OutPacket(ClientHeader.ClientRequestKey);
        packet.Encode<ClientRequestKey>(new() {Channel = id});
        client.Send(packet);
    }
}