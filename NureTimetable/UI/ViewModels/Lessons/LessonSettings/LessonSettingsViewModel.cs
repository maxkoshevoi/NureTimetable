using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Cist;
using NureTimetable.DAL.Models;
using NureTimetable.UI.ViewModels.Lessons.LessonSettings;
using System.Collections.ObjectModel;
using Xamarin.CommunityToolkit.ObjectModel;

namespace NureTimetable.UI.ViewModels;

public class LessonSettingsViewModel : BaseViewModel
{
    private bool updatingProgrammatically = false;

    #region Properties
    public LessonInfo LessonInfo { get; }

    private bool? _showLessonIsChecked = false;
    public bool? ShowLessonIsChecked { get => _showLessonIsChecked; set => SetProperty(ref _showLessonIsChecked, value); }

    public ObservableCollection<CheckedEntity<EventType>> LvEventTypes { get; set; }

    public ObservableCollection<CheckedEntity<Teacher>> LvTeachers { get; set; }

    public Command ShowLessonStateChangedCommand { get; }
    public IAsyncCommand BackButtonPressedCommand { get; }
    #endregion

    public LessonSettingsViewModel(LessonInfo lessonInfo, TimetableInfo timetable, bool saveOnExit)
    {
        LessonInfo = lessonInfo;
        updatingProgrammatically = true;
        ShowLessonIsChecked = lessonInfo.Settings.Hiding.ShowLesson;
        updatingProgrammatically = false;

        LvEventTypes = new
        (
            timetable.EventTypes(lessonInfo.Lesson.ID)
                .Select(et => new CheckedEntity<EventType>(et, EventTypeStateChanged))
                .OrderBy(et => et.Entity.ShortName)
        );
        LvTeachers = new
        (
            timetable.Teachers(lessonInfo.Lesson.ID)
                .Select(t => new CheckedEntity<Teacher>(t, TeacherStateChanged))
                .OrderBy(et => et.Entity.ShortName)
        );
        UpdateEventTypesCheck(true);

        ShowLessonStateChangedCommand = CommandFactory.Create(ShowLessonStateChanged);
        BackButtonPressedCommand = CommandFactory.Create(async () =>
        {
            if (saveOnExit)
            {
                await EventsRepository.UpdateLessonsInfo(timetable);
            }
            await Shell.Current.GoToAsync("..", true);
        });
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
            foreach (var eventType in LvEventTypes)
            {
                eventType.IsChecked = (bool)LessonInfo.Settings.Hiding.ShowLesson;
            }
            foreach (var teacher in LvTeachers)
            {
                teacher.IsChecked = (bool)LessonInfo.Settings.Hiding.ShowLesson;
            }
        }
        else
        {
            foreach (var eventType in LvEventTypes)
            {
                eventType.IsChecked = !LessonInfo.Settings.Hiding.EventTypesToHide.Contains(eventType.Entity.ID);
            }
            foreach (var teacher in LvTeachers)
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
        if (LessonInfo.Settings.Hiding.EventTypesToHide.None() &&
            LessonInfo.Settings.Hiding.TeachersToHide.None())
        {
            return true;
        }
        else if ((LvEventTypes.Any() && LessonInfo.Settings.Hiding.EventTypesToHide.Count == LvEventTypes.Count) ||
            (LvTeachers.Any() && LessonInfo.Settings.Hiding.TeachersToHide.Count == LvTeachers.Count))
        {
            return false;
        }
        return null;
    }
}
