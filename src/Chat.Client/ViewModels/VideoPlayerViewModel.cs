using System;
using Avalonia.Threading;
using LibVLCSharp.Shared;

namespace Chat.Client.ViewModels;

public class VideoPlayerViewModel : ViewModelBase
{
    public MediaPlayer MediaPlayer { get; }

    public Action CloseAction { get; set; }

    public bool IsDisposed { get; private set; }

    private readonly LibVLC _libVlc = new();

    public VideoPlayerViewModel()
    {
        Core.Initialize();

        MediaPlayer = new MediaPlayer(_libVlc)
        {
            EnableHardwareDecoding = true
        };
        MediaPlayer.EndReached += MediaPlayerOnEndReached;
    }

    public void Play(string path)
    {
        using var media = new Media(_libVlc, path);
        MediaPlayer.Play(media);
    }

    private void MediaPlayerOnEndReached(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(CloseAction);
        Dispose();
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        MediaPlayer?.Dispose();
        _libVlc?.Dispose();
        GC.Collect();
        IsDisposed = true;
    }
}