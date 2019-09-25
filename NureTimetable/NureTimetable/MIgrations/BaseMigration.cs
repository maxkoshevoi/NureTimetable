using NureTimetable.Core.Models.Consts;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace NureTimetable.Migrations
{
    public abstract class BaseMigration
    {
        public static IReadOnlyCollection<BaseMigration> Migrations => new[] 
        {
            new CanSelectMultipleEntitiesMigration()
        };

        public abstract bool IsNeedsToBeApplied();

        public abstract bool Apply();

        protected static bool HandleException(Func<bool> func)
        {
            try
            {
                return func?.Invoke() ?? false;
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                });
            }
            return false;
        }
    }
}
