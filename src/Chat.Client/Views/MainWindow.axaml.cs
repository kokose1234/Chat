using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
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
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, vm => vm.Channels, v => v.ChannelList.Items)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.SearchUsers, v => v.SearchUserList.Items)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.SelectedChannel, v => v.ChannelList.SelectedItem)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.SelectedUser, v => v.SearchUserList.SelectedItem)
                    .DisposeWith(disposables);
            });
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

        private void MenuButton_OnPointerEnter(object? sender, PointerEventArgs e)
        {
            var asset = _assets.Open(new Uri($"avares://{_assemblyName}/Assets/Buttons/edit-hover.png"));
            MenuImage.Source = new Bitmap(asset);
        }

        private void MenuButton_OnPointerLeave(object? sender, PointerEventArgs e)
        {
            var asset = _assets.Open(new Uri($"avares://{_assemblyName}/Assets/Buttons/edit.png"));
            MenuImage.Source = new Bitmap(asset);
        }

        private void MenuButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            ViewModel?.EditProfileCommand.Execute().Subscribe();
        }

        private void AttachButton_OnPointerEnter(object? sender, PointerEventArgs e)
        {
            var asset = _assets.Open(new Uri($"avares://{_assemblyName}/Assets/Buttons/attach-hover.png"));
            AttachImage.Source = new Bitmap(asset);
        }

        private void AttachButton_OnPointerLeave(object? sender, PointerEventArgs e)
        {
            var asset = _assets.Open(new Uri($"avares://{_assemblyName}/Assets/Buttons/attach.png"));
            AttachImage.Source = new Bitmap(asset);
        }

        private void AttachImage_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
            ViewModel?.AttachCommand.Execute().Subscribe();
        }

        private void PlayButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
            ViewModel?.ResumeCommand.Execute().Subscribe();
            ViewModel?.SendResumeMusicPacket();
        }

        private void PauseButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
            ViewModel?.PauseCommand.Execute().Subscribe();
            ViewModel?.SendPauseMusicPacket();
        }

        private void ShowUserInfo_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            ViewModel?.OpenUserInfoCommand.Execute().Subscribe();
        }

        public void ScrollToBottom() => ChatScrollViewer?.ScrollToEnd();
    }
}