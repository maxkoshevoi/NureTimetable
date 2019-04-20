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
        Group group;

        public ManageLessonsPage(Group group)
        {
            InitializeComponent();
            Title += $": {group.Name}";
            this.group = group;

            TimetableInfo timetable = EventsRepository.GetTimetableLocal(new SavedEntity(group));
            if (timetable == null)
            {
                return;
            }
            List<LessonInfo> lessonInfo = timetable.LessonsInfo;
            lessons = new ObservableCollection<LessonInfo>
            (
                timetable.Lessons()
                    .Select(lesson =>
                    {
                        LessonInfo res = lessonInfo.FirstOrDefault(li => li.Lesson == lesson)
                            ?? new LessonInfo { Lesson = lesson };
                        //res.EventTypesInfo = timetable.EventTypes(lesson.ID)
                        //    .Select(et => res.EventTypesInfo.FirstOrDefault(eti => eti.Name == et) ?? new EventTypeInfo { Name = et })
                        //    .ToList();
                        return res;
                    })
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
                DisplayAlert("Управление предметами", "Для управления предметами необходимо сначала загрузить расписание группы.", "Ok");
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
            
            await Navigation.PushAsync(new LessonSettingsPage(selectedLesson));
        }

        private void Save_Clicked(object sender, EventArgs e)
        {
            EventsRepository.UpdateLessonsInfo(new SavedEntity(group), lessons.ToList());
            DisplayAlert("Сохранение настроек", $"Настройки предметов для группы {group.Name} успешно сохранены.", "Ok");
            Navigation.PopAsync();
        }
    }
}
