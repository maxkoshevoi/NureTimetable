using NureTimetable.Core.Models;
using NureTimetable.UI.Helpers;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels
{
    public class BaseViewModel : NotifyPropertyChangedBase
    {
        private protected INavigation Navigation => Shell.Current.Navigation;

        private LocalizedString title;
        public LocalizedString Title { get => title; set => SetProperty(ref title, value); }
    }
}