namespace NureTimetable.UI.ViewModels;

public abstract class BaseViewModel : ObservableObject
{
    private protected INavigation Navigation => Shell.Current.Navigation;

    private LocalizedString? title;
    public LocalizedString Title
    {
        get => title ?? throw new NullReferenceException();
        set => SetProperty(ref title, value);
    }
}
