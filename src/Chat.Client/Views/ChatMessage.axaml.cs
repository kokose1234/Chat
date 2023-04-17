using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Chat.Client.Views;

public partial class ChatMessage : UserControl
{
    public ChatMessage()
    {
        InitializeComponent();
    }

    // protected override Size MeasureOverride(Size availableSize)
    // {
    //     var border = (Border) Content;
    //     var textBlock = (TextBlock) border.Child;
    //
    //     textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
    //
    //     // Add some padding to the calculated size to make room for the border
    //     var desiredWidth = textBlock.DesiredSize.Width + border.BorderThickness.Left + border.BorderThickness.Right;
    //     var desiredHeight = textBlock.DesiredSize.Height + border.BorderThickness.Top + border.BorderThickness.Bottom;
    //
    //     return new Size(desiredWidth, desiredHeight);
    // }
}