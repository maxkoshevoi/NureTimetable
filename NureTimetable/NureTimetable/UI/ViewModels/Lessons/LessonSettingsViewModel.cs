using NureTimetable.DAL.Models.Local;
using NureTimetable.Services.Helpers;
using NureTimetable.UI.ViewModels.Core;
using Syncfusion.XForms.Buttons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Lessons
{
    public class LessonSettingsViewModel : BaseViewModel
    {
        #region Classes
        public class ListViewViewModel<T> : BaseViewModel
        {
            private ObservableCollection<CheckedEntity<T>> _itemSource;
            private bool _isVisible;
            private double _heightRequest;

            public ObservableCollection<CheckedEntity<T>> ItemsSource { get => _itemSource; internal set => SetProperty(ref _itemSource, value, onChanged: ItemsSourceChanged); }
            public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
            public double HeightRequest { get => _heightRequest; set => SetProperty(ref _heightRequest, value); }

            public ListViewViewModel(INavigation navigation) : base(navigation)
            {}
            
            private void ItemsSourceChanged()
            {
                if (ItemsSource == null || ItemsSource.Count == 0)
                {
                    IsVisible = false;
                }
                else
                {
                    Resize();
                    IsVisible = true;
                }
            }

            private void Resize()
            {
                const int rowHeight = 43,
                    headerHeight = 19;

                // ListView doesn't support AutoSize, so we have to do it manually
                HeightRequest = rowHeight * ItemsSource.Count + headerHeight;
            }
        }

        public class CheckedEntity<T> : BaseViewModel
        {
            private T _entity;
            private bool? _isChecked;
            private readonly Action _stateChanged;
            
            public T Entity { get => _entity; set => SetProperty(ref _entity, value); }
            public bool? IsChecked { get => _isChecked; set => SetProperty(ref _isChecked, value, onChanged: _stateChanged); }

            public CheckedEntity(INavigation navigation, Action<CheckedEntity<T>> stateChanged = null) : base(navigation)
            {
                _stateChanged = () => stateChanged?.Invoke(this);
            }
        }
        #endregion

        #region Variables
        LessonInfo lessonInfo;
        bool updatingProgrammatically = false;

        private bool? _showLessonIsChecked = false;
        private string _lessonNotesText;
        private string _title;
        #endregion

        #region Properties
        public bool? ShowLessonIsChecked { get => _showLessonIsChecked; set => SetProperty(ref _showLessonIsChecked, value); }
        public string LessonNotesText { get => _lessonNotesText; set => SetProperty(ref _lessonNotesText, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public ListViewViewModel<EventType> LvEventTypes { get; set; }
        public ListViewViewModel<Teacher> LvTeachers { get; set; }

        public ICommand ShowLessonStateChangedCommand { get; }
        public ICommand LessonNotesTextChangedCommand { get; }
        #endregion

        public LessonSettingsViewModel(INavigation navigation, LessonInfo lessonInfo, TimetableInfo timetableInfo) : base(navigation)
        {
            Title = lessonInfo.Lesson.FullName;

            this.lessonInfo = lessonInfo;
            updatingProgrammatically = true;
            ShowLessonIsChecked = lessonInfo.Settings.Hiding.ShowLesson;
            LessonNotesText = lessonInfo.Notes;
            updatingProgrammatically = false;

            LvEventTypes = new ListViewViewModel<EventType>(Navigation)
            {
                ItemsSource = new ObservableCollection<CheckedEntity<EventType>>(timetableInfo.EventTypes(lessonInfo.Lesson.ID)
                    .Select(et => new CheckedEntity<EventType>(Navigation, EventTypeStateChanged)
                    {
                        Entity = et
                    })
                    .OrderBy(et => et.Entity.ShortName))
            };
            LvTeachers = new ListViewViewModel<Teacher>(Navigation)
            {
                ItemsSource = new ObservableCollection<CheckedEntity<Teacher>>(timetableInfo.Teachers(lessonInfo.Lesson.ID)
                    .Select(t => new CheckedEntity<Teacher>(Navigation, TeacherStateChanged)
                    {
                        Entity = t,
                    })
                    .OrderBy(et => et.Entity.ShortName))
            };

            UpdateEventTypesCheck();

            ShowLessonStateChangedCommand = CommandHelper.CreateCommand(ShowLessonStateChanged);
            LessonNotesTextChangedCommand = CommandHelper.CreateCommand(LessonNotesTextChanged);

        }
        
        private void EventTypeStateChanged(CheckedEntity<EventType> e)
        {
            lessonInfo.Settings.Hiding.EventTypesToHide.RemoveAll(id => id == e.Entity.ID);
            if (e.IsChecked == false)
            {
                lessonInfo.Settings.Hiding.EventTypesToHide.Add(e.Entity.ID);
            }
            UpdateShowLessonCheck();
        }

        private void TeacherStateChanged(CheckedEntity<Teacher> e)
        {
            lessonInfo.Settings.Hiding.TeachersToHide.RemoveAll(id => id == e.Entity.ID);
            if (e.IsChecked == false)
            {
                lessonInfo.Settings.Hiding.TeachersToHide.Add(e.Entity.ID);
            }
            UpdateShowLessonCheck();
        }

        private void ShowLessonStateChanged()
        {
            lessonInfo.Settings.Hiding.ShowLesson = ShowLessonIsChecked;
            UpdateEventTypesCheck();

            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(this, "OneLessonSettingsChanged", lessonInfo);
            });
        }

        private void UpdateEventTypesCheck()
        {
            if (updatingProgrammatically) return;

            updatingProgrammatically = true;
            if (lessonInfo.Settings.Hiding.ShowLesson != null)
            {
                foreach (var eventType in LvEventTypes.ItemsSource)
                {
                    eventType.IsChecked = (bool)lessonInfo.Settings.Hiding.ShowLesson;
                }
                foreach (var teacher in LvTeachers.ItemsSource)
                {
                    teacher.IsChecked = (bool)lessonInfo.Settings.Hiding.ShowLesson;
                }
            }
            else
            {
                foreach (var eventType in LvEventTypes.ItemsSource)
                {
                    eventType.IsChecked = !lessonInfo.Settings.Hiding.EventTypesToHide.Contains(eventType.Entity.ID);
                }
                foreach (var teacher in LvTeachers.ItemsSource)
                {
                    teacher.IsChecked = !lessonInfo.Settings.Hiding.TeachersToHide.Contains(teacher.Entity.ID);
                }
            }
            updatingProgrammatically = false;
        }

        private void UpdateShowLessonCheck()
        {
            if (updatingProgrammatically) return;

            updatingProgrammatically = true;
            if (lessonInfo.Settings.Hiding.EventTypesToHide.Count == 0 && lessonInfo.Settings.Hiding.TeachersToHide.Count == 0)
            {
                ShowLessonIsChecked = true;
            }
            else if (lessonInfo.Settings.Hiding.EventTypesToHide.Count == LvEventTypes.ItemsSource.Count 
                && lessonInfo.Settings.Hiding.TeachersToHide.Count == LvTeachers.ItemsSource.Count)
            {
                ShowLessonIsChecked = false;
            }
            else
            {
                ShowLessonIsChecked = null;
            }
            updatingProgrammatically = false;
        }

        private void LessonNotesTextChanged()
        {
            lessonInfo.Notes = LessonNotesText;
        }
    }
}
