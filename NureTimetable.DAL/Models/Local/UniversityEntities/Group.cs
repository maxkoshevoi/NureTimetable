namespace NureTimetable.DAL.Models.Local
{
    public class Group
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public BaseEntity<long> Faculty { get; set; }
        public BaseEntity<long> Direction { get; set; }
        public BaseEntity<long>? Speciality { get; set; }
    }
}
