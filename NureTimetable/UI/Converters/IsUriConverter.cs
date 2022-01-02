using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Globalization;

namespace NureTimetable.UI.Converters;

public class IsUriConverter : IValueConverter, IMarkupExtension
{
    public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is string str && Uri.IsWellFormedUriString(str, UriKind.Absolute);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    public object ProvideValue(IServiceProvider serviceProvider) => this;
}
