using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.UI.Converters
{
    public class ToolbarIconValueConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            new FontImageSource
            {
                FontFamily = (OnPlatform<string>)Application.Current.Resources["MaterialFontFamily"],
                Glyph = (string)value
            };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();

        public object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
