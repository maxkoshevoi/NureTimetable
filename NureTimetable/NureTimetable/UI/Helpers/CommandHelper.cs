using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NureTimetable.UI.Helpers
{
    public static class CommandHelper
    {
        public static Command Create<T>(Func<T, Task> action) 
            => new Command<T>(async param => await action(param));

        public static Command Create<T>(Func<T, Task> action, Func<T, bool> canExecute)
            => new Command<T>(async param => await action(param), canExecute);

        public static Command Create(Func<Task> action)
            => new Command(async () => await action());

        public static Command Create(Func<Task> action, Func<bool> canExecute)
            => new Command(async () => await action(), canExecute);

        public static Command Create<T>(Action<T> action)
            => new Command<T>(action);

        public static Command Create<T>(Action<T> action, Func<T, bool> canExecute)
            => new Command<T>(action, canExecute);

        public static Command Create(Action action)
            => new Command(action);

        public static Command Create(Action action, Func<bool> canExecute)
            => new Command(action, canExecute);
    }
}