using NureTimetable.Core.Localization;
using NureTimetable.DAL.Moodle;
using NureTimetable.DAL.Settings;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels
{
    public class DlNureLoginViewModel : BaseViewModel
    {
        private readonly MoodleRepository moodleRepository = new();

        #region Properties
        private string _login = string.Empty;
        public string Login { get => _login; set => SetProperty(ref _login, value); }

        private string _password = string.Empty;
        public string Password { get => _password; set => SetProperty(ref _password, value); }

        public LocalizedString LoggedInAs { get; } = new(() => string.Format(LN.LoggedInAs, SettingsRepository.Settings.DlNureUser?.FullName, SettingsRepository.Settings.DlNureUser?.Id));

        public IAsyncCommand LoginCommand { get; }
        public Command LogoutCommand { get; }
        #endregion

        public DlNureLoginViewModel()
        {
            var currectUser = SettingsRepository.Settings.DlNureUser;
            if (currectUser != null)
            {
                Login = currectUser.Login;
                Password = currectUser.Password;
            }

            LoginCommand = CommandFactory.Create(OnLogin, allowsMultipleExecutions: false);
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
}