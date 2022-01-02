using System.Globalization;

namespace NureTimetable;

public static class LinkerPreserve
{
    [Obsolete("Don't call this method. It's only needed for linker.", true)]
    public static void Cultures()
    {
        // These classes won't be linked away because of the code.
        //
        // This is to resolve crash at CultureInfo.CurrentCulture
        // when language is set to Thai. See
        // https://github.com/xamarin/Xamarin.Forms/issues/4037

        _ = new ChineseLunisolarCalendar();
        _ = new HebrewCalendar();
        _ = new HijriCalendar();
        _ = new JapaneseCalendar();
        _ = new JapaneseLunisolarCalendar();
        _ = new KoreanCalendar();
        _ = new KoreanLunisolarCalendar();
        _ = new PersianCalendar();
        _ = new TaiwanCalendar();
        _ = new TaiwanLunisolarCalendar();
        _ = new ThaiBuddhistCalendar();
        _ = new UmAlQuraCalendar();
    }
}
