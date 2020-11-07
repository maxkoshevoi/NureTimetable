﻿using NureTimetable.Core.Localization;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Lessons.ManageLessons
{
    public class ManageLessonsViewModel : BaseViewModel
    {
        #region Variables
        private bool _isNoSourceLayoutVisable;

        private readonly TimetableInfo timetable;

        private ObservableCollection<LessonViewModel> _lessons;
        #endregion

        #region Properties
        public bool HasUnsavedChanes { get; set; } = false;

        public bool IsNoSourceLayoutVisable { get => _isNoSourceLayoutVisable; set => SetProperty(ref _isNoSourceLayoutVisable, value); }
        
        public ObservableCollection<LessonViewModel> Lessons { get => _lessons; private set => SetProperty(ref _lessons, value); }

        public ICommand PageAppearingCommand { get; }
        public ICommand SaveClickedCommand { get; }
        public ICommand BackButtonPressedCommand { get; }
        #endregion

        public ManageLessonsViewModel(SavedEntity savedEntity)
        {
            Title = $"{LN.Lessons}: {savedEntity.Name}";

            timetable = EventsRepository.GetTimetableLocal(savedEntity);
            if (timetable is null)
            {
                IsNoSourceLayoutVisable = true;
            }
            else
            {
                Lessons = new ObservableCollection<LessonViewModel>
                (
                    timetable.Lessons()
                        .Select(lesson => timetable.LessonsInfo.FirstOrDefault(li => li.Lesson == lesson) ?? new LessonInfo { Lesson = lesson })
                        .OrderBy(lesson => !timetable.Events.Where(e => e.Start >= DateTime.Now).Any(e => e.Lesson == lesson.Lesson))
                        .ThenBy(lesson => lesson.Lesson.ShortName)
                        .Select(lesson => new LessonViewModel(lesson, timetable, this))
                );
                IsNoSourceLayoutVisable = Lessons.Count == 0;
            }

            PageAppearingCommand = CommandHelper.Create(PageAppearing);
            SaveClickedCommand = CommandHelper.Create(SaveClicked);
            BackButtonPressedCommand = CommandHelper.Create(BackButtonPressed);
        }

        private async Task PageAppearing()
        {
            if (Lessons != null)
            {
                return;
            }

            await Shell.Current.DisplayAlert(LN.LessonsManagement, LN.AtFirstLoadTimetable, LN.Ok);
            await Navigation.PopAsync();
        }

        private async Task SaveClicked()
        {
            if (Lessons is null)
            {
                await Shell.Current.DisplayAlert(LN.LessonsManagement, LN.AtFirstLoadTimetable, LN.Ok);
                return;
            }

            EventsRepository.UpdateLessonsInfo(timetable.Entity, Lessons.Select(l => l.LessonInfo).ToList());
            HasUnsavedChanes = false;

            await Shell.Current.DisplayAlert(LN.SavingSettings, string.Format(LN.EntityLessonSettingsSaved, timetable.Entity.Name), LN.Ok);
            await Navigation.PopAsync();
        }

        private async Task BackButtonPressed()
        {
            bool canClose = true;
            if (HasUnsavedChanes)
            {
                canClose = await Shell.Current.DisplayAlert(LN.UnsavedChangesTitle, LN.UnsavedChangesMessage, LN.Yes, LN.Cancel);
            }

            if (canClose)
            {
                await Navigation.PopAsync();
            }
        }
    }
}