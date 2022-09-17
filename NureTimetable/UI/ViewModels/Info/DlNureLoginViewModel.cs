using NureTimetable.DAL.Moodle;

namespace NureTimetable.UI.ViewModels;

public partial class DlNureLoginViewModel : BaseViewModel
{
    private readonly MoodleRepository moodleRepository = new();

    #region Properties
    [ObservableProperty] private string _login = string.Empty;
    [ObservableProperty] private string _password = string.Empty;

    public LocalizedString LoggedInAs { get; } = new(() => string.Format(LN.LoggedInAs, SettingsRepository.Settings.DlNureUser?.FullName, SettingsRepository.Settings.DlNureUser?.Id));

    public IRelayCommand LoginCommand { get; }
    public IRelayCommand LogoutCommand { get; }
    #endregion

    public DlNureLoginViewModel()
    {
        var currectUser = SettingsRepository.Settings.DlNureUser;
        if (currectUser != null)
        {
            Login = currectUser.Login;
            Password = currectUser.Password;
        }

        LoginCommand = CommandFactory.Create(OnLogin);
        LogoutCommand = CommandFactory.Create(OnLogout);
    }

    public async Task OnLogin()
    {
        try
        {
            await moodleRepository.AuthenticateAsync(Login, Password);
            OnPropertyChanged(nameof(LoggedInAs));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(LN.SomethingWentWrong, ex.Message, LN.Ok);
        }
    }

    public void OnLogout()
    {
        SettingsRepository.Settings.DlNureUser = null;
        Login = string.Empty;
        Password = string.Empty;
    }
}
