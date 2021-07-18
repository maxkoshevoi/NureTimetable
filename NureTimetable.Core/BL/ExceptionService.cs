using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Maui.Essentials;
using NureTimetable.Core.Models.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace NureTimetable.Core.BL
{
    public static class ExceptionService
    {
        public delegate void ExceptionHandler(Exception exception);
        public static event ExceptionHandler? ExceptionLogged;

        public static void LogException(Exception ex)
        {
            ExceptionLogged?.Invoke(ex);

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
                if (Connectivity.NetworkAccess == Microsoft.Maui.Essentials.NetworkAccess.None)
                {
                    // No Internet caused WebException, nothing to log here
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

                if (!properties.ContainsKey("Status"))
                {
                    properties.Add("Status", cistEx.Status.ToString());
                }

                Analytics.TrackEvent("CistException", properties);
                return;
            }
            else if (ex is IOException && ex.Message.StartsWith("Disk full."))
            {
                return;
            }

            // Logging exception
            Crashes.TrackError(ex, properties, attachments.ToArray());
        }
    }
}
