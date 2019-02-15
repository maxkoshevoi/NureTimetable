using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NureTimetable.Models
{
    public class LessonSettings : INotifyPropertyChanged
    {
        public string LessonName { get; set; }
        public List<string> EventTypes { get; set; } = new List<string>();
        public bool IsSomeSettingsApplied
            => HidingSettings.ShowLesson != true;

        public LessonHidingSettings HidingSettings { get; } = new LessonHidingSettings();

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HidingSettings)));
        }
        #endregion
    }
}
