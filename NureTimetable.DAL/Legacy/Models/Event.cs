using System;
using Xamarin.Forms;

namespace NureTimetable.DAL.Legacy.Models
{
    class Event
    {
        public string Lesson { get; set; }
        public string Room { get; set; }
        public string Type { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string DisplayInfo
            => $"{Lesson}{Environment.NewLine}{Room} {Type}{Environment.NewLine}{Start:HH:mm} - {End:HH:mm}";

        public Color Color
        {
            get
            {
                //Color baseColor = (Color) App.Current.Resources[ResourceManager.KeyForEventColor(Type)];
                //if (End <= DateTime.Now)
                //{
                //    baseColor = Color.FromRgb(baseColor.R * 0.9, baseColor.G * 0.9, baseColor.B * 0.9);
                //}
                //return baseColor;
                return Color.DeepSkyBlue;
            }
        }
    }
}
