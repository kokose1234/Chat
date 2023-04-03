using System;
using System.IO;
using Chat.Client.ViewModels;
using NAudio.Wave;

namespace Chat.Client.Data;

public sealed class SoundPlayer : IDisposable
{
    public float Volume
    {
        get => _waveOut.Volume;
        set
        {
            if (value >= 0 && value <= 1.0)
            {
                _waveOut.Volume = value;
            }
        }
    }

    public int Position
    {
        get => (int) (_mp3Reader.Position / _mp3Reader.WaveFormat.AverageBytesPerSecond);
        set => _mp3Reader.Seek(value * _mp3Reader.WaveFormat.AverageBytesPerSecond, SeekOrigin.Begin);
    }

    public int Length => (int) _mp3Reader.TotalTime.TotalSeconds;

    public bool Disposed { get; private set; }

    private readonly Stream _stream;
    private readonly Mp3FileReader _mp3Reader;
    private readonly WaveOut _waveOut;
    private readonly MainWindowViewModel _viewModel;

    public SoundPlayer(byte[] data, MainWindowViewModel viewModel)
    {
        _stream = new MemoryStream(data, false);
        _waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());

        try
        {
            _mp3Reader = new Mp3FileReader(_stream);
            _waveOut.Init(_mp3Reader);
        }
        catch (Exception ex)
        {
            throw ex;
        }

        Volume = 0.75f;
        _waveOut.PlaybackStopped += WaveOutOnPlaybackStopped;
        _viewModel = viewModel;
    }

    private void WaveOutOnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (!Disposed)
        {
            Stop();
            Dispose();
        }
    }

    public void Play() => _waveOut.Play();

    public void Pause() => _waveOut.Pause();

    public void Stop() => _waveOut.Stop();

    public void Dispose()
    {
        // _waveOut.Dispose();
        _mp3Reader.Dispose();
        _stream.Dispose();
        Disposed = true;

        _viewModel.IsMusicPaused = false;
        _viewModel.IsPlayingMusic = false;
    }
}