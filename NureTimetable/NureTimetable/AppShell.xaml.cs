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
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using AppTheme = NureTimetable.Core.Models.Settings.AppTheme;

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
                if (SettingsRepository.Settings.Theme == AppTheme.FollowSystem)
                {
                    ThemeHelper.SetAppTheme((AppTheme)e.RequestedTheme);
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

        public static async Task PerformMigrations()
        {
            if (!VersionTracking.IsFirstLaunchForCurrentBuild)
                return;

            List<BaseMigration> migrationsToApply = BaseMigration.Migrations.Where(m => m.IsNeedsToBeApplied()).ToList();
            if (migrationsToApply.Any())
            {
                // Not Shell.Current.DisplayAlert cause Shell.Current is null here
                await App.Current.MainPage.DisplayAlert(LN.FinishingUpdateTitle, LN.FinishingUpdateDescription, LN.Ok);
                bool isSuccess = true;
                foreach (var migration in migrationsToApply)
                {
                    if (!migration.Apply())
                    {
                        isSuccess = false;
                    }
                }
                if (isSuccess)
                {
                    App.Current.MainPage = new AppShell();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert(LN.FinishingUpdateTitle, LN.FinishingUpdateFail, LN.Ok);
                }
            }
        }

        private static void LogException(Exception ex)
        {
            // Getting exception Data
            Dictionary<string, string> properties = new();
            List<ErrorAttachmentLog> attachments = new();
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
            else if (ex is CistException cistEx)
            {
                // CistException happens for external reasons, and shouldn't be treated as an exception.
                // But just in case it is logged as Event
                
                properties.Add("Status", cistEx.Status.ToString());

                Analytics.TrackEvent("CistException", properties);
                return;
            }

            // Logging exception
            Crashes.TrackError(ex, properties, attachments.ToArray());
        }
    }
}