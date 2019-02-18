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

        public string DisplayInfo => $"{Lesson} {Room} {Type}";

        public Color Color
        {
            get => (Color) App.Current.Resources[$"{Type.ToLowerInvariant()}Color"];
        }
    }
}
