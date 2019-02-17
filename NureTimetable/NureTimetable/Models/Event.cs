using System;
using Xamarin.Forms;

namespace NureTimetable.Models
{
    public class Event
    {
        public string Lesson { get; set; }
        public string Room { get; set; }
        public string Type { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string DisplayInfo
            => $"{Lesson} {Room} {Type}";

        public Color Color
        {
            get
            {
                switch (Type.ToLower())
                {
                    case "лк":
                        return Color.FromRgb(255, 228, 161);
                    case "пз":
                        return Color.FromRgb(226, 240, 181);
                    case "лб":
                        return Color.FromRgb(229, 203, 242);
                    case "конс":
                        return Color.FromRgb(142, 216, 241);
                    case "зал":
                        return Color.FromRgb(255, 156, 167);
                    case "іспкомб":
                        return Color.FromRgb(244, 108, 105);
                    default:
                        return Color.LightSteelBlue;
                }
            }
        }
    }
}
