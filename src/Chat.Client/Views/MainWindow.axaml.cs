using Avalonia.Input;
using Avalonia.ReactiveUI;
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

        private void TopBar_OnPointerPressed(object? sender, PointerPressedEventArgs e) => BeginMoveDrag(e);
    }
}