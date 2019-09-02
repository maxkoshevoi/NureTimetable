using NureTimetable.Core.Models.Consts;
using System;
using Xamarin.Forms;

namespace NureTimetable.MIgrations
{
    public abstract class BaseMigration
    {
        public static BaseMigration[] Migrations =
        {
            new CanSelectMultipleEntitiesMigration()
        };

        public abstract bool IsNeedsToBeApplied();

        public abstract bool Apply();

        protected static bool HandleException(Func<bool> func)
        {
            try
            {
                return func();
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
