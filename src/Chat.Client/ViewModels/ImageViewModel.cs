using System;
using System.IO;
using Avalonia.Media.Imaging;
using Chat.Client.Database.Repositories;
using Chat.Client.Database;

namespace Chat.Client.ViewModels;

public class ImageViewModel : ViewModelBase, IDisposable
{
    public Bitmap Image { get; }

    private readonly MemoryStream _imageStream;

    public ImageViewModel(uint id)
    {
        var repo = DatabaseManager.GetRepository<ImageRepository>();
        using var tempStream = new MemoryStream();
        repo.GetImage(id, tempStream);

        var data = tempStream.ToArray();
        _imageStream = new MemoryStream(data);
        Image = new Bitmap(_imageStream);
    }

    public void Dispose()
    {
        _imageStream.Dispose();
        Image.Dispose();
    }
}