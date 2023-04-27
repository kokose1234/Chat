using Avalonia.Media.Imaging;
using Chat.Client.Data.Types;

namespace Chat.Client.ViewModels;

public class MessageViewModel : ViewModelBase
{
    public string? Username { get; }

    public string Message { get; }

    public string? Date { get; }

    public bool IsMine { get; }

    public bool IsImage => Image != null;

    public bool IsAlert => Type == MessageType.Alert;

    public Bitmap? Image { get; }

    public MessageType Type { get; }

    public MessageViewModel()
    {
        // Username = "Test";
        // Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In id risus lacus. Donec eget euismod libero, et pellentesque odio. Duis.";
        // Date = "오전 6:07";
        // IsMine = true;
        // Type = MessageType.Message;

        Message = "경고";
        Type = MessageType.Alert;
    }


    public MessageViewModel(string sender, string message, string date, bool isMe, string? image = null)
    {
        Username = sender;
        Message = message;
        Date = date;
        IsMine = isMe;
        Type = image != null ? MessageType.Image : MessageType.Message;
        if (image != null) Image = new Bitmap(image);
    }

    public MessageViewModel(string message)
    {
        Message = message;
        Type = MessageType.Alert;
    }
}