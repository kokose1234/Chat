using System;
using System.Reflection;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Chat.Client.ViewModels;

namespace Chat.Client.Views;

public partial class AddFriendWindow : ReactiveWindow<UserInfoViewModel>
{
    private readonly string _assemblyName = Assembly.GetEntryAssembly()?.GetName().Name!;
    private readonly IAssetLoader _assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;

    public AddFriendWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        Close();
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

    private void AddFriendButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ViewModel == null) return;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        if (ViewModel.IsFriend) ViewModel.RemoveFriendCommand.Execute().Subscribe();
        else ViewModel.AddFriendCommand.Execute().Subscribe();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ViewModel == null) return;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        ViewModel.StartChatCommand.Execute().Subscribe();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        ViewModel?.Dispose();
    }
}