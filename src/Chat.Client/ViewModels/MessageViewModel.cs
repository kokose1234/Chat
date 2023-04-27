using System;
using System.IO;
using Avalonia.Media.Imaging;
using Chat.Client.Data.Types;
using Chat.Client.Database.Repositories;
using Chat.Client.Database;

namespace Chat.Client.ViewModels;

public class MessageViewModel : ViewModelBase, IDisposable
{
    public string? Username { get; }

    public string Message { get; }

    public string? Date { get; }

    public bool IsMine { get; }

    public bool IsImage { get; } = false;

    public bool IsAlert => Type == MessageType.Alert;

    public Bitmap? Image { get; }

    public MessageType Type { get; }

    private MemoryStream _imageStream;

    public MessageViewModel()
    {
        Username = "Test";
        Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In id risus lacus. Donec eget euismod libero, et pellentesque odio. Duis.";
        Date = "오전 6:07";
        IsMine = true;
        Type = MessageType.Message;

        // Message = "경고";
        // Type = MessageType.Alert;
    }


    public MessageViewModel(string sender, string message, string date, bool isMe, uint? imageId = null)
    {
        Username = sender;
        Message = message;
        Date = date;
        IsMine = isMe;
        Type = imageId != null ? MessageType.Image : MessageType.Message;

        if (imageId != null)
        {
            var repo = DatabaseManager.GetRepository<ImageRepository>();
            using var tempStream = new MemoryStream();
            repo.GetImage(imageId.Value, tempStream);

            var data = tempStream.ToArray();
            _imageStream = new MemoryStream(data);
            Image = new Bitmap(_imageStream);
            IsImage = true;
        }
    }

    public MessageViewModel(string message)
    {
        Message = message;
        Type = MessageType.Alert;
    }

    public void Dispose()
    {
        _imageStream.Dispose();
        Image?.Dispose();
    }
}