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
                        return Color.FromRgb(255, 204, 128);
                    case "пз":
                        return Color.FromRgb(157, 242, 115);
                    case "лб":       
                        return Color.FromRgb(217, 167, 241);
                    case "конс":     
                        return Color.FromRgb(152, 220, 230);
                    case "зал":      
                        return Color.FromRgb(241, 118, 116);
                    case "іспкомб": 
                        return Color.FromRgb(241, 246, 136);
                    default:
                        return Color.LightSteelBlue;
                }
            }
        }
    }
}
