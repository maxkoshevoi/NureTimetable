using NureTimetable.DataStores;
using NureTimetable.Models;
using NureTimetable.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Views.Lessons
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManageLessonsPage : ContentPage
    {
        public ObservableCollection<LessonSettings> lessons { get; set; }
        Group group;

        public ManageLessonsPage(SavedGroup group)
        {
            InitializeComponent();
            Title += $": {group.Name}";
            this.group = group;

            EventList eventList = EventsDataStore.GetEventsLocal(group.ID);
            if (eventList == null)
            {
                return;
            }
            List<LessonSettings> lessonSettings = LessonSettingsDataStore.GetLessonSettings(group.ID);
            lessons = new ObservableCollection<LessonSettings>
            (
                eventList.Lessons()
                    .Select(lesson =>
                    {
                        LessonSettings res = lessonSettings.FirstOrDefault(ls => ls.LessonName == lesson)
                            ?? new LessonSettings { LessonName = lesson };
                        res.EventTypes = eventList.EventTypes(lesson).ToList();
                        return res;
                    })
                    .OrderBy(lesson => lesson.LessonName)
            );
            LessonsList.ItemsSource = lessons;
            if (lessons.Count == 0)
            {
                NoSourceLayout.IsVisible = true;
            }

            MessagingCenter.Subscribe<LessonSettingsPage, LessonSettings>(this, "OneLessonSettingsChanged", (sender, newLessonSettings) =>
            {
                for (int i = 0; i < lessons.Count; i++)
                {
                    if (lessons[i].LessonName == newLessonSettings.LessonName)
                    {
                        lessons[i] = newLessonSettings;
                        lessons[i].NotifyChanged();
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
            if (e.Item == null || !(e.Item is LessonSettings))
                return;

            LessonSettings selectedLesson = (LessonSettings)e.Item;
            //Deselect Item
            ((ListView)sender).SelectedItem = null;
            
            await Navigation.PushAsync(new LessonSettingsPage(selectedLesson));
        }

        private void Save_Clicked(object sender, EventArgs e)
        {
            LessonSettingsDataStore.UpdateLessonSettings(group.ID, lessons.ToList());
            DisplayAlert("Сохранение настроек", $"Настройки предметов для группы {group.Name} успешно сохранены.", "Ok");
            Navigation.PopAsync();
        }
    }
}
