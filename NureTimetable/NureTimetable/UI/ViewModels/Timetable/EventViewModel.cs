using NureTimetable.DAL.Models.Local;
using NureTimetable.Models.Consts;
using System;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Timetable
{
    public class EventViewModel : Event
    {
        public EventViewModel(Event ev)
        {
            StartUtc = ev.StartUtc;
            EndUtc = ev.EndUtc;
            Groups = ev.Groups;
            Teachers = ev.Teachers;
            RoomName = ev.RoomName;
            Lesson = ev.Lesson;
            PairNumber = ev.PairNumber;
            Type = ev.Type;
        }

        public string DisplayInfo
               => $"{Lesson.ShortName}{Environment.NewLine}{RoomName} {Type.ShortName}{Environment.NewLine}{Start:HH:mm} - {End:HH:mm}";

        public Color Color
        {
            get
            {
                Color baseColor = ResourceManager.EventColor(Type.EnglishBaseName);
                //if (End <= DateTime.Now)
                //{
                //    baseColor = Color.FromRgb(baseColor.R * 0.9, baseColor.G * 0.9, baseColor.B * 0.9);
                //}
                return baseColor;
            }
        }
    }
}
