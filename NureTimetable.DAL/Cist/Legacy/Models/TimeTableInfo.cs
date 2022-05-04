namespace NureTimetable.DAL.Cist.Legacy.Models
{
    [Obsolete("", true)]
    class TimetableInfo
    {
        public Group Group { get; set; } = new();
        public List<Event> Events { get; set; } = new();
        public List<LessonInfo> LessonsInfo { get; set; } = new();

        public TimetableInfo()
        { }

        public TimetableInfo(Group group)
        {
            Group = group;
        }
    }
}
