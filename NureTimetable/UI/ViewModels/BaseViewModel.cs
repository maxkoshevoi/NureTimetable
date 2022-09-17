namespace NureTimetable.UI.ViewModels;

[INotifyPropertyChanged]
public abstract partial class BaseViewModel
{
    private protected INavigation Navigation => Shell.Current.Navigation;

    [ObservableProperty]
    private LocalizedString title = null!;
}
