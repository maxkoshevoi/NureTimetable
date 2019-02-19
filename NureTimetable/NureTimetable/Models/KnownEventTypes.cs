using System.Collections.Generic;

namespace NureTimetable.Models
{
    public static class KnownEventTypes
    {
        public static readonly string[] Values =
        {
            "лк",
            "пз",
            "лб",
            "конс",
            "зал",
            "іспкомб",
        };

        public static string Lecture = Values[0];
        public static string Practice = Values[1];
        public static string LabWork = Values[2];
        public static string Сonsultation = Values[3];
        public static string Credit = Values[4];
        public static string Exam = Values[5];
    }
}
