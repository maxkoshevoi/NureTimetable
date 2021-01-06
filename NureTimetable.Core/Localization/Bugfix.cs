using System;
using System.Globalization;

namespace NureTimetable.Core.Localization
{
    public static class Bugfix
    {
        public static void InitCalendarCrashFix()
        {
            // These classes won't be linked away because of the code,
            // but we also won't have to construct unnecessarily either,
            // hence the if statement with (hopefully) impossible
            // runtime condition.
            //
            // This is to resolve crash at CultureInfo.CurrentCulture
            // when language is set to Thai. See
            // https://github.com/xamarin/Xamarin.Forms/issues/4037
            if (Environment.CurrentDirectory == "_never_POSSIBLE_")
            {
#pragma warning disable CA1806 // Do not ignore method results
                new ChineseLunisolarCalendar();
                new HebrewCalendar();
                new HijriCalendar();
                new JapaneseCalendar();
                new JapaneseLunisolarCalendar();
                new KoreanCalendar();
                new KoreanLunisolarCalendar();
                new PersianCalendar();
                new TaiwanCalendar();
                new TaiwanLunisolarCalendar();
                new ThaiBuddhistCalendar();
                new UmAlQuraCalendar();
#pragma warning restore CA1806 // Do not ignore method results
            }
        }
    }
}
