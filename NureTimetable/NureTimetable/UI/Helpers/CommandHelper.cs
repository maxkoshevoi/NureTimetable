using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.UI.Helpers
{
    public static class CommandHelper
    {
        /// <summary>
        /// The current task
        /// </summary>
        private static Task _currentTask;

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <returns>ICommand.</returns>
        public static ICommand CreateCommand<T>(Func<T, Task> action)
        {
            return new Command<T>(async param =>
            {
                if (_currentTask != null && !_currentTask.IsCompleted)
                {
                    return;
                }

                _currentTask = action?.Invoke(param);

                if (_currentTask != null)
                {
                    await _currentTask;
                }
            });
        }

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>ICommand.</returns>
        public static ICommand CreateCommand(Func<Task> action)
        {
            return new Command(async () =>
            {
                if (_currentTask != null && !_currentTask.IsCompleted)
                {
                    return;
                }

                _currentTask = action?.Invoke();

                if (_currentTask != null)
                {
                    await _currentTask;
                }
            });
        }

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <returns>ICommand.</returns>
        public static ICommand CreateCommand<T>(Action<T> action)
        {
            return new Command<T>(action);
        }

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>ICommand.</returns>
        public static ICommand CreateCommand(Action action)
        {
            return new Command(action);
        }
    }
}