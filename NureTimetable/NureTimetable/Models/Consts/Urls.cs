using System;

namespace NureTimetable.Models.Consts
{
    public static class Urls
    {
        public enum CistTimetableType { Html = 1, Xls = 2, Csv = 3 }

        public static string CistTimetableUrl(CistTimetableType type, DateTime dateStart, DateTime dateEnd, params int[] groupIDs) 
            => $"http://cist.nure.ua/ias/app/tt/WEB_IAS_TT_GNR_RASP.GEN_GROUP_POTOK_RASP?ATypeDoc={(int)type}&Aid_group={string.Join("_", groupIDs)}&Aid_potok=0&ADateStart={dateStart.ToString("dd.MM.yyyy")}&ADateEnd={dateEnd.ToString("dd.MM.yyyy")}&AMultiWorkSheet=0";

        public static string CistAllGroupsSource(int? branchID) 
            => $"http://cist.nure.ua/ias/app/tt/WEB_IAS_TT_AJX_GROUPS?p_id_fac={branchID}";
    }
}
