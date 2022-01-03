using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Globalization;

namespace NureTimetable.UI.Converters;

public class ToolbarIconValueConverter : IValueConverter, IMarkupExtension
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        new FontImageSource
        {
            FontFamily = "MaterialFontFamily",
            Glyph = (string)value
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    public object ProvideValue(IServiceProvider serviceProvider) => this;
}
