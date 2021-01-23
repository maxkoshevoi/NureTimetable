using NureTimetable.Core.Extensions;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using System.Linq;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Lessons.LessonSettings
{
    public class LessonSettingsViewModel : BaseViewModel
    {
        private bool updatingProgrammatically = false;

        #region Properties
        public LessonInfo LessonInfo { get; }
        
        private bool? _showLessonIsChecked = false;
        public bool? ShowLessonIsChecked { get => _showLessonIsChecked; set => SetProperty(ref _showLessonIsChecked, value); }
        
        public ListViewViewModel<EventType> LvEventTypes { get; set; }
        
        public ListViewViewModel<Teacher> LvTeachers { get; set; }

        public Command ShowLessonStateChangedCommand { get; }
        #endregion

        public LessonSettingsViewModel(LessonInfo lessonInfo, TimetableInfo timetableInfo)
        {
            LessonInfo = lessonInfo;
            updatingProgrammatically = true;
            ShowLessonIsChecked = lessonInfo.Settings.Hiding.ShowLesson;
            updatingProgrammatically = false;

            LvEventTypes = new ListViewViewModel<EventType>()
            {
                ItemsSource = { timetableInfo.EventTypes(lessonInfo.Lesson.ID)
                    .Select(et => new CheckedEntity<EventType>(et, EventTypeStateChanged))
                    .OrderBy(et => et.Entity.ShortName) }
            };
            LvTeachers = new()
            {
                ItemsSource = { timetableInfo.Teachers(lessonInfo.Lesson.ID)
                    .Select(t => new CheckedEntity<Teacher>(t, TeacherStateChanged))
                    .OrderBy(et => et.Entity.ShortName) }
            };
            UpdateEventTypesCheck(true);

            ShowLessonStateChangedCommand = CommandFactory.Create(ShowLessonStateChanged);
        }
        
        private void EventTypeStateChanged(CheckedEntity<EventType> e)
        {
            LessonInfo.Settings.Hiding.EventTypesToHide.RemoveAll(id => id == e.Entity.ID);
            if (e.IsChecked == false)
            {
                LessonInfo.Settings.Hiding.EventTypesToHide.Add(e.Entity.ID);
            }
            UpdateShowLessonCheck();
        }

        private void TeacherStateChanged(CheckedEntity<Teacher> e)
        {
            LessonInfo.Settings.Hiding.TeachersToHide.RemoveAll(id => id == e.Entity.ID);
            if (e.IsChecked == false)
            {
                LessonInfo.Settings.Hiding.TeachersToHide.Add(e.Entity.ID);
            }
            UpdateShowLessonCheck();
        }

        private void ShowLessonStateChanged()
        {
            LessonInfo.Settings.Hiding.ShowLesson = ShowLessonIsChecked;
            UpdateEventTypesCheck();
            MessagingCenter.Send(this, MessageTypes.OneLessonSettingsChanged, LessonInfo);
        }

        private void UpdateEventTypesCheck(bool force = false)
        {
            if (updatingProgrammatically || (!force && ShowLessonIsChecked == IsShowEvents())) return;

            updatingProgrammatically = true;
            if (LessonInfo.Settings.Hiding.ShowLesson != null)
            {
                foreach (var eventType in LvEventTypes.ItemsSource)
                {
                    eventType.IsChecked = (bool)LessonInfo.Settings.Hiding.ShowLesson;
                }
                foreach (var teacher in LvTeachers.ItemsSource)
                {
                    teacher.IsChecked = (bool)LessonInfo.Settings.Hiding.ShowLesson;
                }
            }
            else
            {
                foreach (var eventType in LvEventTypes.ItemsSource)
                {
                    eventType.IsChecked = !LessonInfo.Settings.Hiding.EventTypesToHide.Contains(eventType.Entity.ID);
                }
                foreach (var teacher in LvTeachers.ItemsSource)
                {
                    teacher.IsChecked = !LessonInfo.Settings.Hiding.TeachersToHide.Contains(teacher.Entity.ID);
                }
            }
            updatingProgrammatically = false;
        }

        private void UpdateShowLessonCheck()
        {
            if (updatingProgrammatically) return;

            updatingProgrammatically = true;
            ShowLessonIsChecked = IsShowEvents();
            updatingProgrammatically = false;
        }

        /// <returns>true = all, false = none, null = some</returns>
        private bool? IsShowEvents()
        {
            if (LessonInfo.Settings.Hiding.EventTypesToHide.Count == 0 && 
                LessonInfo.Settings.Hiding.TeachersToHide.Count == 0)
            {
                return true;
            }
            else if ((LvEventTypes.ItemsSource.Count > 0 && LessonInfo.Settings.Hiding.EventTypesToHide.Count == LvEventTypes.ItemsSource.Count) ||
                (LvTeachers.ItemsSource.Count > 0 && LessonInfo.Settings.Hiding.TeachersToHide.Count == LvTeachers.ItemsSource.Count))
            {
                return false;
            }
            return null;
        }
    }
}
