using System;
using System.Reactive;
using Avalonia.Threading;
using Chat.Client.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using LibVLCSharp.Shared;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Chat.Client.ViewModels;

public class VideoPlayerViewModel : ViewModelBase
{
    public MediaPlayer? MediaPlayer { get; }

    public Action CloseAction { get; set; }

    public bool IsDisposed { get; private set; }

    [Reactive]
    public int Length { get; private set; }

    [Reactive]
    public bool IsPlaying { get; private set; }

    [Reactive]
    public bool IsPaused { get; private set; }

    public ReactiveCommand<Unit, Unit> ResumeCommand { get; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }

    private readonly uint _channel;
    private int _position;

    public int Position
    {
        get => (int?) (MediaPlayer?.Position * Length) ?? 0;
        set
        {
            if (MediaPlayer == null) return;
            if (value < 0)
            {
                var time = Math.Abs(value);

                MediaPlayer.Position = (float) time / Length;
                this.RaiseAndSetIfChanged(ref _position, time);
            }
            else
            {
                if (Math.Abs((int) (MediaPlayer.Position * Length) - value) < 1)
                {
                    this.RaiseAndSetIfChanged(ref _position, value);
                    return;
                }

                MediaPlayer.Position = (float) value / Length;
                WarpVideo(value);
                this.RaiseAndSetIfChanged(ref _position, value);
            }
        }
    }

    private int _volume;

    public int Volume
    {
        get => MediaPlayer?.Volume ?? 75;
        set
        {
            if (MediaPlayer == null) return;
            MediaPlayer.Volume = value;
            this.RaiseAndSetIfChanged(ref _volume, value);
        }
    }

    private readonly LibVLC _libVlc = new();

    public VideoPlayerViewModel(uint channel)
    {
        Core.Initialize();

        MediaPlayer = new MediaPlayer(_libVlc)
        {
            EnableHardwareDecoding = true
        };
        MediaPlayer.EndReached += MediaPlayerOnEndReached;

        ResumeCommand = ReactiveCommand.Create(Resume);
        PauseCommand = ReactiveCommand.Create(Pause);
        _channel = channel;
    }

    public void Play(string path)
    {
        using var media = new Media(_libVlc, path);
        Length = (int) MediaPlayer.Length / 1000;
        IsPlaying = true;
        MediaPlayer.Play(media);
        MediaPlayer.PositionChanged += (sender, args) =>
        {
            Length = (int) MediaPlayer.Length / 1000;
            Position = (int) (args.Position * Length);
        };
    }

    private void WarpVideo(int position)
    {
        using var packet = new OutPacket(ClientHeader.ClientSyncVideo);
        var request = new ClientSyncVideo {Channel = _channel, Data = position};

        packet.Encode(request);
        ChatClient.Instance.Send(packet);
    }

    public void SendResumeVideoPacket()
    {
        using var packet = new OutPacket(ClientHeader.ClientSyncVideo);
        var request = new ClientSyncVideo {Channel = _channel, Data = int.MinValue + 1};

        packet.Encode(request);
        ChatClient.Instance.Send(packet);
    }

    public void SendPauseVideoPacket()
    {
        using var packet = new OutPacket(ClientHeader.ClientSyncVideo);
        var request = new ClientSyncVideo {Channel = _channel, Data = int.MinValue + 2};

        packet.Encode(request);
        ChatClient.Instance.Send(packet);
    }

    public void Resume()
    {
        MediaPlayer?.Play();
        IsPaused = false;
    }

    public void Pause()
    {
        MediaPlayer?.Pause();
        IsPaused = true;
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