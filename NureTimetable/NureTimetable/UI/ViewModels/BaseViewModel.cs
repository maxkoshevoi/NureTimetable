using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private protected INavigation Navigation => Shell.Current.Navigation;
        public event PropertyChangedEventHandler PropertyChanged;
        
        string title = string.Empty;
        public string Title { get => title; set => SetProperty(ref title, value); }

        private readonly object lockObject = new object();

        protected bool SetProperty<T>(ref T backingStore,
            T value,
            Action onChanged = null,
            [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                return false;
            }

            lock (lockObject)
            {
                backingStore = value;
                onChanged?.Invoke();
                OnPropertyChanged(propertyName);
            }

            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propName = "") 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}