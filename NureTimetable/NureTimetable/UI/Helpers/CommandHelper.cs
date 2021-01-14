using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.Helpers
{
    public static class CommandHelper
    {
        public static IAsyncCommand Create(
            Func<Task> action,
            Func<bool> canExecute = null,
            Action<Exception> onException = null,
            bool continueOnCapturedContext = false,
            bool allowsMultipleExecutions = true)
        {
            Func<object, bool> formattedCanExecute = canExecute is null ? null : _ => canExecute();
            return new AsyncCommand(action, formattedCanExecute, onException, continueOnCapturedContext, allowsMultipleExecutions);
        }

        public static IAsyncCommand Create<TCanExecute>(
            Func<Task> action,
            Func<TCanExecute, bool> canExecute = null,
            Action<Exception> onException = null,
            bool continueOnCapturedContext = false,
            bool allowsMultipleExecutions = true)
        {
            Func<object, bool> formattedCanExecute = canExecute is null ? null : arg => canExecute((TCanExecute)arg);
            return new AsyncCommand(action, formattedCanExecute, onException, continueOnCapturedContext, allowsMultipleExecutions);
        }

        public static IAsyncCommand<TExecute> Create<TExecute>(
            Func<TExecute, Task> action,
            Func<TExecute, bool> canExecute = null,
            Action<Exception> onException = null,
            bool continueOnCapturedContext = false,
            bool allowsMultipleExecutions = true) => 
            Create<TExecute, TExecute>(action, canExecute, onException, continueOnCapturedContext, allowsMultipleExecutions);

        public static IAsyncCommand<TExecute, TCanExecute> Create<TExecute, TCanExecute>(
            Func<TExecute, Task> action,
            Func<TCanExecute, bool> canExecute = null,
            Action<Exception> onException = null,
            bool continueOnCapturedContext = false,
            bool allowsMultipleExecutions = true)
        {
            return new AsyncCommand<TExecute, TCanExecute>(action, canExecute, onException, continueOnCapturedContext, allowsMultipleExecutions);
        }

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