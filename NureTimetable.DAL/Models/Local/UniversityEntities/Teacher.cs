namespace NureTimetable.DAL.Models.Local
{
    public class Teacher
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public BaseEntity<long> Department { get; set; }
    }
}
