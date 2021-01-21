using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.Helpers
{
    public static class CommandHelper
	{
		public static Command Create(Action execute, Func<bool> canExecute = null) =>
			new Command(execute, canExecute ?? (() => true));

		public static Command Create<TExecute>(Action<TExecute> execute, Func<bool> canExecute = null) =>
			new Command(e => execute((TExecute)e), canExecute is null ? _ => true : _ => canExecute());

		public static Command Create<TExecute, TCanExecute>(Action<TExecute> execute, Func<TCanExecute, bool> canExecute) =>
			new Command(e => execute((TExecute)e), e => canExecute((TCanExecute)e));

		public static IAsyncCommand Create(
			Func<Task> execute,
			Func<bool> canExecute = null,
			Action<Exception> onException = null,
			bool continueOnCapturedContext = false,
			bool allowsMultipleExecutions = true) =>
			new AsyncCommand(execute, canExecute is null ? null : _ => canExecute(), onException, continueOnCapturedContext, allowsMultipleExecutions);

		public static IAsyncCommand<TExecute> Create<TExecute>(
			Func<TExecute, Task> execute,
			Func<bool> canExecute = null,
			Action<Exception> onException = null,
			bool continueOnCapturedContext = false,
			bool allowsMultipleExecutions = true) =>
			new AsyncCommand<TExecute>(execute, canExecute is null ? null : _ => canExecute(), onException, continueOnCapturedContext, allowsMultipleExecutions);

		public static IAsyncCommand<TExecute, TCanExecute> Create<TExecute, TCanExecute>(
			Func<TExecute, Task> execute,
			Func<TCanExecute, bool> canExecute = null,
			Action<Exception> onException = null,
			bool continueOnCapturedContext = false,
			bool allowsMultipleExecutions = true) =>
			new AsyncCommand<TExecute, TCanExecute>(execute, canExecute, onException, continueOnCapturedContext, allowsMultipleExecutions);
	}
}