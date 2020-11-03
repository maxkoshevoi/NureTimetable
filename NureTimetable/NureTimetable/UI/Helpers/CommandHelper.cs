using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.UI.Helpers
{
    public static class CommandHelper
    {
        public static ICommand CreateCommand<T>(Func<T, Task> action)
        {
            return new Command<T>(async param => await action(param));
        }

        public static ICommand CreateCommand(Func<Task> action)
        {
            return new Command(async () => await action());
        }

        public static ICommand CreateCommand<T>(Action<T> action)
        {
            return new Command<T>(action);
        }

        public static ICommand CreateCommand(Action action)
        {
            return new Command(action);
        }
    }
}