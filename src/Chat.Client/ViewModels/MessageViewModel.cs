using System;
using System.IO;
using System.Reactive;
using Avalonia.Media.Imaging;
using Chat.Client.Data.Types;
using Chat.Client.Database.Repositories;
using Chat.Client.Database;
using ReactiveUI;
using Avalonia.Threading;
using Chat.Client.Views;

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

    public ReactiveCommand<Unit, Unit>? OpenImageCommand { get; }


    private readonly MemoryStream _imageStream;
    private readonly uint? _imageId;

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
            repo.GetThumbnailImage(imageId.Value, tempStream);

            var data = tempStream.ToArray();
            _imageStream = new MemoryStream(data);
            Image = new Bitmap(_imageStream);
            IsImage = true;
            _imageId = imageId;

            OpenImageCommand = ReactiveCommand.Create(OpenImage);
        }
    }

    public MessageViewModel(string message)
    {
        Message = message;
        Type = MessageType.Alert;
    }

    private void OpenImage()
    {
        if (_imageId == null) return;

        Dispatcher.UIThread.Post(() =>
        {
            var window = new ImageWindow
            {
                DataContext = new ImageViewModel(_imageId.Value)
            };
            window.Show();
        });
    }

    public void Dispose()
    {
        _imageStream.Dispose();
        Image?.Dispose();
        OpenImageCommand?.Dispose();
    }
}