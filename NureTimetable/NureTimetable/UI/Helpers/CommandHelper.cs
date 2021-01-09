using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.Helpers
{
    public static class CommandHelper
    {
        public static IAsyncCommand Create(Func<Task> action) =>
            new AsyncCommand(action);

        public static IAsyncCommand Create(Func<Task> action, Func<bool> canExecute) =>
            new AsyncCommand(action, _ => canExecute());

        public static IAsyncCommand<T> Create<T>(Func<T, Task> action) => 
            new AsyncCommand<T>(action);

        public static IAsyncCommand<T> Create<T>(Func<T, Task> action, Func<T, bool> canExecute) => 
            Create<T, T>(action, canExecute);

        public static IAsyncCommand<TExecute, TCanExecute> Create<TExecute, TCanExecute>(Func<TExecute, Task> action, Func<TCanExecute, bool> canExecute) =>
            new AsyncCommand<TExecute, TCanExecute>(action, canExecute);

        public static Command Create(Action action) =>
            new Command(action);

        public static Command Create(Action action, Func<bool> canExecute) =>
            new Command(action, canExecute);

        public static Command<T> Create<T>(Action<T> action) => 
            new Command<T>(action);

        public static Command<T> Create<T>(Action<T> action, Func<T, bool> canExecute) => 
            new Command<T>(action, canExecute);
    }
}