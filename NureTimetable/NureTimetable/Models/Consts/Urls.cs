using System;

namespace NureTimetable.Models.Consts
{
    public static class Urls
    {
        public static string CistTimetableUrl(DateTime dateStart, DateTime dateEnd, params int[] groupIDs) 
            => $"http://cist.nure.ua/ias/app/tt/WEB_IAS_TT_GNR_RASP.GEN_GROUP_POTOK_RASP?ATypeDoc=3&Aid_group={string.Join("_", groupIDs)}&Aid_potok=0&ADateStart={dateStart.ToString("dd.MM.yyyy")}&ADateEnd={dateEnd.ToString("dd.MM.yyyy")}&AMultiWorkSheet=0";

        public const string CistAllKNGroupsSource = "http://cist.nure.ua/ias/app/tt/f?p=778:2:274725030663670::NO";
    }
}
