using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace NureTimetable.Models
{
    public class Event
    {
        private string type;

        public string Lesson { get; set; }
        public string Room { get; set; }
        public string Type { get => type; set => type = value.ToLowerInvariant(); }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string DisplayInfo => $"{Lesson} {Room} {Type}";
        private Color DefaultColor => Color.LightSteelBlue;

        public Color Color
        {
            get
            {
                return KnownEventTypes.Values.Contains(Type)
                    ? (Color) App.Current.Resources[$"{Type}Color"]
                    : DefaultColor;
            }
        }
    }
}
