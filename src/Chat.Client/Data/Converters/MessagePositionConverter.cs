using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Chat.Client.Data.Types;
using Chat.Client.ViewModels;

namespace Chat.Client.Data.Converters;

public class MessagePositionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MessageViewModel msg)
        {
            switch (msg.Type)
            {
                case MessageType.Alert:
                    return HorizontalAlignment.Center;
                case MessageType.Message:
                case MessageType.Image:
                    return msg.IsMine ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}