using System;
using System.Linq;
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
        {
            get => $"{Lesson} {Room} {Type}";
        }

        public Color Color
        {
            get => (Color) App.Current.Resources[this.ResourceKeyForColor()];
        }
    }
}
