using NureTimetable.Helpers;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace NureTimetable.DAL
{
    public static class LessonSettingsDataStore
    {
        public static List<LessonSettings> GetLessonSettings(int groupID)
        {
            var lessonSettings = new List<LessonSettings>();
            string filePath = FilePath.LessonSettings(groupID);
            if (!File.Exists(filePath))
            {
                return lessonSettings;
            }

            lessonSettings = Serialisation.FromJsonFile<List<LessonSettings>>(filePath) ?? lessonSettings;
            return lessonSettings;
        }

        public static void UpdateLessonSettings(int groupID, List<LessonSettings> lessonSettings)
        {
            Serialisation.ToJsonFile(lessonSettings, FilePath.LessonSettings(groupID));
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(Application.Current, MessageTypes.LessonSettingsChanged, groupID);
            });
        }
    }
}
