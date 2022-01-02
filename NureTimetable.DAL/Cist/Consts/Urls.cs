using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Models;

namespace NureTimetable.DAL.Cist.Consts
{
    public static class Urls
    {
        public static Uri CistApiEntityTimetable(TimetableEntityType entity, long entityId, DateTime dateStart, DateTime dateEnd) =>
            new($"https://cist.nure.ua/ias/app/tt/P_API_EVEN_JSON" +
                $"?type_id={(int)entity}" +
                $"&timetable_id={entityId}" +
                $"&time_from={new DateTimeOffset(dateStart.Date).ToUnixTimeSeconds()}" +
                $"&time_to={new DateTimeOffset(dateEnd.Date.AddDays(1)).ToUnixTimeSeconds()}" +
                $"&idClient={Keys.CistApiKey}");

        public static Uri CistApiAllGroups =>
            new($"https://cist.nure.ua/ias/app/tt/P_API_GROUP_JSON");

        public static Uri CistApiAllTeachers =>
            new($"https://cist.nure.ua/ias/app/tt/P_API_PODR_JSON");

        public static Uri CistApiAllRooms =>
            new($"https://cist.nure.ua/ias/app/tt/P_API_AUDITORIES_JSON");

        public static Uri CistSiteAllGroups(long? facultyId) =>
            new($"https://cist.nure.ua/ias/app/tt/WEB_IAS_TT_AJX_GROUPS?p_id_fac={facultyId}");

        public static Uri CistSiteAllTeachers(long facultyId = -1, long kafId = -1) =>
            new($"https://cist.nure.ua/ias/app/tt/WEB_IAS_TT_AJX_TEACHS?p_id_fac={facultyId}&p_id_kaf={kafId}");

        public static Uri CistSiteEmptyTimetable =>
            new($"https://cist.nure.ua/ias/app/tt/f?p=778:201:3666577568788626:::201:P201_FIRST_DATE,P201_LAST_DATE,P201_GROUP,P201_POTOK:01.01.1000,01.01.1000,0,0:");
    }
}
