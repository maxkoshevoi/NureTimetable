using NureTimetable.DAL.Models.Local;
using NureTimetable.Models.Consts;
using System;
using Xamarin.Forms;

namespace NureTimetable.Models
{
    class EventViewModel : Event
    {
        public EventViewModel(Event ev)
        {
            Start = ev.Start;
            End = ev.End;
            Groups = ev.Groups;
            Teachers = ev.Teachers;
            RoomName = ev.RoomName;
            Lesson = ev.Lesson;
            PairNumber = ev.PairNumber;
            Type = ev.Type;
        }

        public string DisplayInfo
               => $"{Lesson.ShortName}{Environment.NewLine}{RoomName} {Type.FullName}{Environment.NewLine}{Start.ToString("HH:mm")} - {End.ToString("HH:mm")}";

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
