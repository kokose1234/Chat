using System;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Chat.Client.ViewModels;

namespace Chat.Client.Views;

public partial class ChatMessage : ReactiveUserControl<MessageViewModel>
{
    public ChatMessage()
    {
        InitializeComponent();
    }

    private void Image_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ViewModel?.OpenImageCommand?.Execute().Subscribe();
    }
}