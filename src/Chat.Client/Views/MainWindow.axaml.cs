using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Chat.Client.Net;
using Chat.Client.ViewModels;
using ReactiveUI;

namespace Chat.Client.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private readonly string _assemblyName = Assembly.GetEntryAssembly().GetName().Name!;
        private readonly IAssetLoader _assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;

        public MainWindow()
        {
            this.WhenActivated(disposables => { });

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

            ChatClient.Instance.DisconnectAndStop();
            Close();
            Environment.Exit(0);
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
    }
}