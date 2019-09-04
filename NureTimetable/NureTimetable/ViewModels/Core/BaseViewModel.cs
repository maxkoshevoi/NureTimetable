using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.Core
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public INavigation Navigation;
        public event PropertyChangedEventHandler PropertyChanged;

        protected BaseViewModel(INavigation navigation)
        {
            Navigation = navigation;
        }
        
        private object lockObject = new object();

        protected bool SetProperty<T>(ref T backingStore,
            T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
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
        protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}