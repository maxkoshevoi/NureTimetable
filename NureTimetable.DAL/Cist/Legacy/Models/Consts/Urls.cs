using System;

namespace NureTimetable.DAL.Cist.Legacy.Models.Consts
{
    [Obsolete("", true)]
    static class Urls
    {
        #region Legacy
        public enum CistTimetableFormat { Html = 1, Xls = 2, Csv = 3 }

        public static string CistGroupTimetableUrl(CistTimetableFormat format, DateTime dateStart, DateTime dateEnd, params int[] groupIDs) =>
            $"http://cist.nure.ua/ias/app/tt/WEB_IAS_TT_GNR_RASP.GEN_GROUP_POTOK_RASP?ATypeDoc={(int)format}&Aid_group={string.Join("_", groupIDs)}&Aid_potok=0&ADateStart={dateStart:dd.MM.yyyy}&ADateEnd={dateEnd:dd.MM.yyyy}&AMultiWorkSheet=0";

        public static string CistTeacherTimetableUrl(CistTimetableFormat format, DateTime dateStart, DateTime dateEnd, params int[] TeacherIDs) =>
            $"http://cist.nure.ua/ias/app/tt/WEB_IAS_TT_GNR_RASP.GEN_GROUP_POTOK_RASP?ATypeDoc={(int)format}&Aid_sotr={string.Join("_", TeacherIDs)}&Aid_kaf=0&ADateStart={dateStart:dd.MM.yyyy}&ADateEnd={dateEnd:dd.MM.yyyy}&AMultiWorkSheet=0";

        public static string CistAllGroupsSource(int? branchID) =>
            $"http://cist.nure.ua/ias/app/tt/WEB_IAS_TT_AJX_GROUPS?p_id_fac={branchID}";
        #endregion
    }
}
