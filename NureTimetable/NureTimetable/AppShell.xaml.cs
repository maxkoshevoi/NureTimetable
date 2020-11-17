using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.Exceptions;
using NureTimetable.DAL;
using NureTimetable.Migrations;
using NureTimetable.UI.Themes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xamarin.Essentials;
using Xamarin.Forms;
using Settings = NureTimetable.Core.Models.Settings;

namespace NureTimetable.UI.Views
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Adding theme change handler
            ThemeHelper.SetAppTheme(SettingsRepository.Settings.Theme);
            App.Current.RequestedThemeChanged += (_, e) =>
            {
                if (SettingsRepository.Settings.Theme == Settings.AppTheme.FollowSystem)
                {
                    ThemeHelper.SetAppTheme((Settings.AppTheme)e.RequestedTheme);
                }
            };

            MessagingCenter.Subscribe<Application, Exception>(this, MessageTypes.ExceptionOccurred, (sender, ex) =>
            {
                if (App.IsDebugMode)
                {
                    MainThread.BeginInvokeOnMainThread(async () => 
                        await Shell.Current.DisplayAlert(LN.ErrorDetails, ex.ToString(), LN.Ok)
                    );
                }

                LogException(ex);
            });
        }

        private bool isFirstNavigation = true;

        protected override async void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            if (!isFirstNavigation)
            {
                return;
            }
            isFirstNavigation = false;

            // Log currect timetable view mode
            Analytics.TrackEvent("Timetable view mode", new Dictionary<string, string>
            {
                { nameof(SettingsRepository.Settings.TimetableViewMode), SettingsRepository.Settings.TimetableViewMode.ToString() }
            });

            // Processing migrations
            if (!VersionTracking.IsFirstLaunchForCurrentBuild)
            {
                return;
            }

            List<BaseMigration> migrationsToApply = BaseMigration.Migrations.Where(m => m.IsNeedsToBeApplied()).ToList();
            if (migrationsToApply.Any())
            {
                await Shell.Current.DisplayAlert(LN.FinishingUpdateTitle, LN.FinishingUpdateDescription, LN.Ok);
                bool isSuccess = true;
                foreach (var migration in migrationsToApply)
                {
                    if (!migration.Apply())
                    {
                        isSuccess = false;
                    }
                }
                if (!isSuccess)
                {
                    await Shell.Current.DisplayAlert(LN.FinishingUpdateTitle, LN.FinishingUpdateFail, LN.Ok);
                }
            }
        }

        private static void LogException(Exception ex)
        {
            // Getting exception Data
            var properties = new Dictionary<string, string>();
            var attachments = new List<ErrorAttachmentLog>();
            foreach (DictionaryEntry de in ex.Data)
            {
                if (de.Value is ErrorAttachmentLog attachment)
                {
                    attachments.Add(attachment);
                    continue;
                }
                properties.Add(de.Key.ToString(), de.Value.ToString());
            }
            if (ex.InnerException != null)
            {
                attachments.Add(ErrorAttachmentLog.AttachmentWithText(ex.InnerException.ToString(), "InnerException.txt"));
            }

            // Special cases for certain exception types
            if (ex is WebException webEx)
            {
                if (Connectivity.NetworkAccess == NetworkAccess.None)
                {
                    // No internet caused WebException, nothing to log here
                    return;
                }

                // WebException happens for external reasons, and shouldn't be treated as an exception.
                // But just in case it is logged as Event

                if (webEx.Status != 0 && webEx.Status != WebExceptionStatus.UnknownError)
                {
                    properties.Add("Status", webEx.Status.ToString());
                }
                if (webEx.InnerException != null)
                {
                    properties.Add("InnerException", webEx.InnerException.GetType().FullName);
                }
                properties.Add("Message", ex.Message);

                Analytics.TrackEvent("WebException", properties);
                return;
            }
            else if (ex is CistOutOfMemoryException)
            {
                // CistOutOfMemoryException happens for external reasons, and shouldn't be treated as an exception.
                // But just in case it is logged as Event

                Analytics.TrackEvent("CistOutOfMemoryException", properties);
                return;
            }

            // Logging exception
            Crashes.TrackError(ex, properties, attachments.ToArray());
        }
    }
}