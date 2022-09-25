namespace NureTimetable.Migrations;

public abstract class BaseMigration
{
    public static IReadOnlyCollection<BaseMigration> Migrations => new BaseMigration[]
    {
        new MoveEntityInsideSavedEntityMigration(),
        new RemoveTimelineViewMode(),
        new Removerussian(),
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
            ExceptionService.LogException(ex);
        }
        return false;
    }
}
