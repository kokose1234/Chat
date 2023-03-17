using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Remote;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Chat.Client.Net;
using Chat.Client.ViewModels;
using ReactiveUI;

namespace Chat.Client.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
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
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                ChatClient.Instance.DisconnectAndStop();
                Close();
                Environment.Exit(0);
            }
        }
    }
}