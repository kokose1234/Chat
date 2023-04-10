using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Chat.Client.ViewModels;
using ReactiveUI;

namespace Chat.Client.Views;

public partial class VideoPlayer : ReactiveWindow<VideoPlayerViewModel>
{
    private readonly string _assemblyName = Assembly.GetEntryAssembly().GetName().Name!;
    private readonly IAssetLoader _assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;

    public VideoPlayer()
    {
        this.WhenActivated(disposable => { ViewModel.CloseAction = Close; });

        InitializeComponent();
    }

    private void TopBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void MinimizeButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            WindowState = WindowState.Minimized;
        }
    }


    private void CloseButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        ViewModel?.Dispose();
        Hide();
    }

    private void MinimizeButton_OnPointerEnter(object? sender, PointerEventArgs e)
    {
        var asset = _assets.Open(new Uri($"avares://{_assemblyName}/Assets/Buttons/minimize-button-hover.png"));
        MinimizeImage.Source = new Bitmap(asset);
    }

    private void MinimizeButton_OnPointerLeave(object? sender, PointerEventArgs e)
    {
        var asset = _assets.Open(new Uri($"avares://{_assemblyName}/Assets/Buttons/minimize-button.png"));
        MinimizeImage.Source = new Bitmap(asset);
    }

    private void CloseButton_OnPointerEnter(object? sender, PointerEventArgs e)
    {
        var asset = _assets.Open(new Uri($"avares://{_assemblyName}/Assets/Buttons/close-button-hover.png"));
        CloseImage.Source = new Bitmap(asset);
    }

    private void CloseButton_OnPointerLeave(object? sender, PointerEventArgs e)
    {
        var asset = _assets.Open(new Uri($"avares://{_assemblyName}/Assets/Buttons/close-button.png"));
        CloseImage.Source = new Bitmap(asset);
    }

    private void PlayButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        ViewModel?.ResumeCommand.Execute().Subscribe();
        ViewModel?.SendResumeVideoPacket();
    }

    private void PauseButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        ViewModel?.PauseCommand.Execute().Subscribe();
        ViewModel?.SendPauseVideoPacket();
    }
}