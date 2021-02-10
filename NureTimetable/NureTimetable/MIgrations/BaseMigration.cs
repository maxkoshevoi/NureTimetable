using NureTimetable.Core.Models.Consts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public Task<bool> IsNeedsToBeApplied() =>
            HandleException(IsNeedsToBeAppliedInternal);

        public Task<bool> Apply() =>
            HandleException(ApplyInternal);

        protected abstract Task<bool> IsNeedsToBeAppliedInternal();

        protected abstract Task<bool> ApplyInternal();

        protected static async Task<bool> HandleException(Func<Task<bool>> func)
        {
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
            }
            return false;
        }
    }
}
