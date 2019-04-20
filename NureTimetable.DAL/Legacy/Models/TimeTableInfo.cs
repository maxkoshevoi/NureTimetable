using System;
using System.Collections.Generic;

namespace NureTimetable.DAL.Legacy.Models
{
    class TimetableInfo
    {
        private Group group = new Group();
        public Group Group
        {
            get => group;
            set => group = value ?? throw new NullReferenceException($"Attemt to set {Group} to null");
        }
        private List<Event> events = new List<Event>();
        public List<Event> Events
        {
            get => events;
            set => events = value ?? throw new NullReferenceException($"Attemt to set {Events} to null");
        }
        private List<LessonInfo> lessonsInfo = new List<LessonInfo>();
        public List<LessonInfo> LessonsInfo
        {
            get => lessonsInfo;
            set => lessonsInfo = value ?? throw new NullReferenceException($"Attemt to set {LessonsInfo} to null");
        } 

        public TimetableInfo()
        { }
        
        public TimetableInfo(Group group)
        {
            Group = group;
        }
    }
}
