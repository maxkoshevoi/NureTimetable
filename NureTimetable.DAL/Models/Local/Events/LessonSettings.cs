using System.ComponentModel;

namespace NureTimetable.DAL.Models.Local
{
    public class LessonSettings : INotifyPropertyChanged
    {
        public bool IsSomeSettingsApplied => Hiding.ShowLesson != true;

        public LessonHidingSettings Hiding { get; } = new();

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Hiding)));
        }
        #endregion
    }
}
