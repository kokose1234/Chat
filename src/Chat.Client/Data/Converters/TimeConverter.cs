using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Chat.Client.Data.Converters;

public class TimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int second)
        {
            var time = new TimeSpan(0, 0, second);
            return time.ToString(@"mm\:ss");
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}