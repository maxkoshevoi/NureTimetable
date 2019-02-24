using System;
using Xamarin.Forms;
using NureTimetable.Models.Consts;

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
            => $"{Lesson}{Environment.NewLine}{Room} {Type}{Environment.NewLine}{Start.ToString("HH:mm")} - {End.ToString("HH:mm")}";

        public Color Color
            => (Color) App.Current.Resources[ResourceManager.KeyForEventColor(Type)];
    }
}
