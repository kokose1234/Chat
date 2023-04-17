using System.IO;
using Avalonia.Media.Imaging;

namespace Chat.Client.ViewModels;

public class MessageViewModel : ViewModelBase
{
    public string? Username { get; }

    public string Message { get; }

    public string Date { get; }

    public bool IsMine { get; }

    public bool IsImage => Image != null;

    public Bitmap? Image { get; }

    public MessageViewModel()
    {
        Username = "Test";
        Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In id risus lacus. Donec eget euismod libero, et pellentesque odio. Duis.";
        Date = "오전 6:07";
        IsMine = true;
    }


    public MessageViewModel(string sender, string message, string date, bool isMe, string? image = null)
    {
        Username = sender;
        Message = message;
        Date = date;
        IsMine = isMe;
        if (image != null) Image = new Bitmap(image);
    }
}