using System.Globalization;

namespace NureTimetable.UI.Converters;

public class IsNotUriConverter : IsUriConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        !(bool)base.Convert(value, targetType, parameter, culture);
}
