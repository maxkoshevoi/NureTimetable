using System;
using System.Globalization;

namespace NureTimetable
{
    public static class LinkerPreserve
    {
        public static void Cultures()
        {
            // These classes won't be linked away because of the code,
            // but we also won't have to construct unnecessarily either,
            // hence the if statement with (hopefully) impossible
            // runtime condition.
            //
            // This is to resolve crash at CultureInfo.CurrentCulture
            // when language is set to Thai. See
            // https://github.com/xamarin/Xamarin.Forms/issues/4037
            if (DateTime.Now.Ticks < 0)
            {
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
    }
}
