using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NureTimetable.Core.Models
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly object lockObject = new();

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

        protected void OnPropertyChanged([CallerMemberName] string propName = "") => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
