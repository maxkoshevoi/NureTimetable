using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Views.Lessons
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManageLessonsPage : ContentPage
    {
        public ObservableCollection<LessonInfo> lessons { get; set; }
        readonly TimetableInfo timetable;

        public ManageLessonsPage(SavedEntity savedEntity)
        {
            InitializeComponent();
            Title += $": {savedEntity.Name}";

            timetable = EventsRepository.GetTimetableLocal(savedEntity);
            if (timetable == null)
            {
                return;
            }
            List<LessonInfo> lessonInfo = timetable.LessonsInfo;
            lessons = new ObservableCollection<LessonInfo>
            (
                timetable.Lessons()
                    .Select(lesson => lessonInfo.FirstOrDefault(li => li.Lesson == lesson) ?? new LessonInfo { Lesson = lesson })
                    .OrderBy(lesson => !timetable.Events.Where(e => e.Start >= DateTime.Now).Any(e => e.Lesson == lesson.Lesson))
                    .ThenBy(lesson => lesson.Lesson.ShortName)
            );
            LessonsList.ItemsSource = lessons;
            if (lessons.Count == 0)
            {
                NoSourceLayout.IsVisible = true;
            }

            MessagingCenter.Subscribe<LessonSettingsPage, LessonInfo>(this, "OneLessonSettingsChanged", (sender, newLessonSettings) =>
            {
                for (int i = 0; i < lessons.Count; i++)
                {
                    if (lessons[i].Lesson == newLessonSettings.Lesson)
                    {
                        lessons[i] = newLessonSettings;
                        lessons[i].Settings.NotifyChanged();
                        break;
                    }
                }
            });
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            if (lessons == null)
            {
                DisplayAlert("Управление предметами", "Для управления предметами необходимо сначала загрузить расписание.", "Ok");
                Navigation.PopAsync();
                return;
            }
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null || !(e.Item is LessonInfo))
                return;

            LessonInfo selectedLesson = (LessonInfo)e.Item;
            //Deselect Item
            ((ListView)sender).SelectedItem = null;
            
            await Navigation.PushAsync(new LessonSettingsPage(selectedLesson, timetable.EventTypes(selectedLesson.Lesson.ID).ToList()));
        }

        private void Save_Clicked(object sender, EventArgs e)
        {
            EventsRepository.UpdateLessonsInfo(timetable.Entity, lessons.ToList());
            DisplayAlert("Сохранение настроек", $"Настройки предметов для \"{timetable.Entity.Name}\" успешно сохранены.", "Ok");
            Navigation.PopAsync();
        }
    }
}
