using NureTimetable.Core.Models.Consts;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace NureTimetable.Migrations
{
    public abstract class BaseMigration
    {
        public static IReadOnlyCollection<BaseMigration> Migrations => new BaseMigration[] 
        {
            new MoveEntityInsideSavedEntityMigration(),
            new RemoveTimelineViewMode(),
        };

        public bool IsNeedsToBeApplied() =>
            HandleException(IsNeedsToBeAppliedInternal);

        public bool Apply() =>
            HandleException(ApplyInternal);

        protected abstract bool IsNeedsToBeAppliedInternal();

        protected abstract bool ApplyInternal();

        protected static bool HandleException(Func<bool> func)
        {
            try
            {
                return func?.Invoke() ?? false;
            }
            catch (Exception ex)
            {
                MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
            }
            return false;
        }
    }
}
