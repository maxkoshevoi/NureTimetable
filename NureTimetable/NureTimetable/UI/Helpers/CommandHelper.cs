using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.Helpers
{
    public static class CommandHelper
    {
        public static IAsyncCommand<T> Create<T>(Func<T, Task> action) => 
            new AsyncCommand<T>(async param => await action(param));

        public static IAsyncCommand<T> Create<T>(Func<T, Task> action, Func<object, bool> canExecute) => 
            new AsyncCommand<T>(async param => await action(param), canExecute);

        public static IAsyncCommand Create(Func<Task> action) => 
            new AsyncCommand(async () => await action());

        public static IAsyncCommand Create(Func<Task> action, Func<object, bool> canExecute) => 
            new AsyncCommand(async () => await action(), canExecute);

        public static Command<T> Create<T>(Action<T> action) => 
            new Command<T>(action);

        public static Command<T> Create<T>(Action<T> action, Func<T, bool> canExecute) => 
            new Command<T>(action, canExecute);

        public static Command Create(Action action) => 
            new Command(action);

        public static Command Create(Action action, Func<bool> canExecute) => 
            new Command(action, canExecute);
    }
}