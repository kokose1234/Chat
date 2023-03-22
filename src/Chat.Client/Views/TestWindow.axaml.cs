using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Chat.Client.ViewModels;

namespace Chat.Client.Views;

public partial class TestWindow : ReactiveWindow<TestViewModel>
{
    public TestWindow()
    {
        InitializeComponent();
    }
}