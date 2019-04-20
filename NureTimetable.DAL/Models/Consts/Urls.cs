using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Models.Local;
using System;

namespace NureTimetable.DAL.Models.Consts
{
    public static class Urls
    {
        public static string CistEntityTimetableUrl(TimetableEntityType entity, long entityId, DateTime dateStart, DateTime dateEnd)
            => $"http://cist.nure.ua/ias/app/tt/P_API_EVEN_JSON" +
                $"?type_id={(int)entity}" +
                $"&timetable_id={entityId}" +
                $"&time_from={new DateTimeOffset(dateStart).ToUnixTimeSeconds()}" +
                $"&time_to={new DateTimeOffset(dateEnd).ToUnixTimeSeconds()}" +
                $"&idClient={Keys.CistApiKey}";

        public static string CistAllGroupsUrl
            => $"http://cist.nure.ua/ias/app/tt/P_API_GROUP_JSON";

        public static string CistAllTeachersUrl
            => $"http://cist.nure.ua/ias/app/tt/P_API_PODR_JSON";

        public static string CistAllRoomsUrl
            => $"http://cist.nure.ua/ias/app/tt/P_API_AUDITORIES_JSON";
    }
}
