using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Models.Local;
using System;

namespace NureTimetable.DAL.Models.Consts
{
    public static class Urls
    {
        public static Uri CistEntityTimetableUrl(TimetableEntityType entity, long entityId, DateTime dateStart, DateTime dateEnd)
            => new Uri($"http://cist.nure.ua/ias/app/tt/P_API_EVEN_JSON" +
                $"?type_id={(int)entity}" +
                $"&timetable_id={entityId}" +
                $"&time_from={new DateTimeOffset(dateStart.Date).ToUnixTimeSeconds()}" +
                $"&time_to={new DateTimeOffset(dateEnd.Date.AddDays(1)).ToUnixTimeSeconds()}" +
                $"&idClient={Keys.CistApiKey}");

        public static Uri CistAllGroupsUrl
            => new Uri($"http://cist.nure.ua/ias/app/tt/P_API_GROUP_JSON");

        public static Uri CistAllTeachersUrl
            => new Uri($"http://cist.nure.ua/ias/app/tt/P_API_PODR_JSON");

        public static Uri CistAllRoomsUrl
            => new Uri($"http://cist.nure.ua/ias/app/tt/P_API_AUDITORIES_JSON");
    }
}
