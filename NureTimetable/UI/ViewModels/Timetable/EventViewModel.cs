using Microsoft.Maui.Graphics;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Models.Consts;
using System;

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

        public bool ShowTime { get; set; } = true;

        public string DisplayInfo 
        {
            get
            {
                string baseInfo = $"{Lesson.ShortName}{Environment.NewLine}{RoomName} {Type.ShortName}";
                if (ShowTime)
                {
                    baseInfo += $"{Environment.NewLine}{Start:HH:mm} - {End:HH:mm}";
                }
                return baseInfo;
            }
        }

        public Color Color => ResourceManager.EventColor(Type.EnglishBaseName);
    }
}
