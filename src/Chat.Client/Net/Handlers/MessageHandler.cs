using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Chat.Common.Packet.Data.Server;
using LiteDB;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerMessage)]
public class MessageHandler : AbstractHandler
{
    private readonly Queue<ServerMessage> _queue = new();
    private readonly ResizeOptions _resizeOptions = new() {Mode = ResizeMode.Max, Position = AnchorPositionMode.Center, Size = new Size(250, 250)};
    private readonly IImageEncoder _encoder = new PngEncoder();

    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var data = inPacket.Decode<ServerMessage>();
        var channel = session.ViewModel.Channels.FirstOrDefault(x => x.Id == data.Message.ChannelId);
        if (channel == null) return Task.CompletedTask;

        var repo = DatabaseManager.GetRepository<ImageRepository>();
        var messageRepo = DatabaseManager.GetRepository<MessageRepository>();

        if (channel is {IsSecret: true, Key.Length: 0})
        {
            _queue.Enqueue(data);
            return Task.CompletedTask;
        }

        _queue.Enqueue(data);

        while (_queue.Count > 0)
        {
            var message = _queue.Dequeue();
            channel = session.ViewModel.Channels.FirstOrDefault(x => x.Id == message.Message.ChannelId);
            if (channel == null) return Task.CompletedTask;

            if (channel is {IsSecret: true, Key.Length: 0})
            {
                _queue.Enqueue(message);
                return Task.CompletedTask;
            }

            if (channel.IsSecret)
            {
                message.Message.Text = Util.Decrypt(message.Message.Text, channel.Key);
                if (message.Message.Attachment != null) message.Message.Attachment = Util.Decrypt(message.Message.Attachment, channel.Key);
            }

            var messageEntity = new MessageEntity
            {
                Id = ObjectId.NewObjectId(),
                ChannelId = message.Message.ChannelId,
                MessageId = message.Message.Id,
                UserId = message.Message.Sender,
                Timestamp = message.Message.Date ?? DateTime.Now,
                Content = Encoding.UTF8.GetString(message.Message.Text),
                HasAttachment = message.Message.Attachment != null
            };
            messageRepo.AddMessage(messageEntity);

            if (message.Message.Attachment == null)
            {
                session.ViewModel.AddMessage(message.Message);
                return Task.CompletedTask;
            }

            var decompressedData = Util.Decompress(message.Message.Attachment);
            var fileData = new byte[decompressedData.Length - 1];
            var fileName = Encoding.UTF8.GetString(message.Message.Text);
            Buffer.BlockCopy(decompressedData, 1, fileData, 0, fileData.Length);

            switch ((AttachmentType) decompressedData[0])
            {
                case AttachmentType.Image:
                {
                    using var image = Image.Load(fileData);
                    using var stream = new MemoryStream(fileData);
                    var randomName = Util.GetRandomFileName();

                    image.Mutate(x => x.Resize(_resizeOptions));
                    image.Save($"./{randomName}.png", _encoder);
                    var thumbnailStream = File.Open($"./{randomName}.png", FileMode.Open);

                    repo.UploadImage(message.Message.Id, stream);
                    repo.UploadThumbnailImage(message.Message.Id, thumbnailStream);

                    thumbnailStream.Dispose();
                    File.Delete($"./{randomName}.png");

                    session.ViewModel.AddImageMessage(message.Message);
                    break;
                }
                case AttachmentType.Music:
                    message.Message.Text = Encoding.UTF8.GetBytes($"{fileName} 재생"); //TODO: 엄준식임
                    session.ViewModel.AddMessage(message.Message);
                    session.ViewModel.PlayMusic(fileData);
                    break;
                case AttachmentType.Video:
                    message.Message.Text = Encoding.UTF8.GetBytes($"{fileName} 재생");
                    session.ViewModel.AddMessage(message.Message);
                    session.ViewModel.PlayVideo(fileName, fileData);
                    break;
            }
        }

        return Task.CompletedTask;
    }
}