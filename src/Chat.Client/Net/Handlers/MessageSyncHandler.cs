using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Chat.Client.Data.Types;
using Chat.Client.Database;
using Chat.Client.Database.Entities;
using Chat.Client.Database.Repositories;
using Chat.Client.Tools;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using LiteDB;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerMessageSync)]
public class MessageSyncHandler : AbstractHandler
{
    private readonly ResizeOptions _resizeOptions = new() { Mode = ResizeMode.Max, Position = AnchorPositionMode.Center, Size = new Size(250, 250) };
    private readonly IImageEncoder _encoder = new PngEncoder();

    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var response = inPacket.Decode<ServerMessageSync>();
        var repo = DatabaseManager.GetRepository<ChannelRepository>();
        var messageRepo = DatabaseManager.GetRepository<MessageRepository>();
        var fileRepo = DatabaseManager.GetRepository<ImageRepository>();

        foreach (var (channelId, data) in response.messages)
        {
            var channel = repo.GetChannel(channelId);
            var lastMessageId = -1;
            if (channel == null) continue;
            if (channel is {IsSecret: true, Key.Length: 0})
            {
                using var packet = new OutPacket(ClientHeader.ClientRequestKey);
                packet.Encode<ClientRequestKey>(new() {Channel = channelId});
                session.Send(packet);
                continue;
            }

            foreach (var message in data.messages)
            {
                if (channel.IsSecret)
                {
                    if (message.Text != null)
                    {
                        message.Text = Util.Decrypt(message.Text, channel.Key);
                    }

                    if (message.Attachment != null)
                    {
                        message.Attachment = Util.Decrypt(message.Attachment, channel.Key);
                    }
                }

                message.Text ??= Array.Empty<byte>();
                lastMessageId = (int) message.Id;
                session.ViewModel.AddMessage(message);

                var messageEntity = new MessageEntity
                {
                    Id = ObjectId.NewObjectId(),
                    MessageId = message.Id,
                    UserId = message.Sender,
                    Timestamp = message.Date.GetValueOrDefault(DateTime.Now),
                    Content = message.Text.Length == 0 ? "" : Encoding.UTF8.GetString(message.Text),
                    HasAttachment = message.Attachment != null,
                };

                if (messageEntity.HasAttachment && message.Attachment?[0] == (byte)AttachmentType.Image)
                {
                    using var image = Image.Load(message.Attachment);
                    using var stream = new MemoryStream(message.Attachment);
                    var randomName = Util.GetRandomFileName();

                    image.Mutate(x => x.Resize(_resizeOptions));
                    image.Save($"./{randomName}.png", _encoder);
                    var thumbnailStream = File.Open($"./{randomName}.png", FileMode.Open);

                    fileRepo.UploadImage(message.Id, stream);
                    fileRepo.UploadThumbnailImage(message.Id, thumbnailStream);

                    thumbnailStream.Dispose();
                    File.Delete($"./{randomName}.png");
                }

                messageRepo.AddMessage(messageEntity);


            }

            if (lastMessageId != -1)
            {
                channel.LastMessageId = (uint) lastMessageId;
                repo.UpdateChannel(channel);
            }
        }

        return Task.CompletedTask;
    }
}