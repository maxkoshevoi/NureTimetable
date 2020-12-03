using NureTimetable.Core.Models;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels
{
    public class BaseViewModel : NotifyPropertyChangedBase
    {
        private protected INavigation Navigation => Shell.Current.Navigation;
        
        string title = string.Empty;
        public string Title { get => title; set => SetProperty(ref title, value); }
    }
}