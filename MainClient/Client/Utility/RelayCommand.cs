using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client.Utility
{
    /// <summary>
    /// A relay command that takes a parameter T
    /// </summary>
    /// <typeparam name="T">paramter for relay command</typeparam>
    public class RelayCommand<T> : ICommand where T : class
    {
        readonly Action<T> action = null;
        readonly Predicate<T> canExecute = null;

        public RelayCommand(Action<T> action)
            : this(action, null)
        {
        }

        public RelayCommand(Action<T> action, Predicate<T> canExecute)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  
        /// If the command does not require data to be passed, this object can be set to null.
        /// </param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter as T);
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object parameter)
        {
            action(parameter as T);
        }
    }


    /// <summary>
    /// A basic command that runs an Action that takes no input
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action action;

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        /// <summary>
        /// Default constructor
        /// </summary>
        public RelayCommand(Action action)
        {
            this.action = action;
        }

        /// <summary>
        /// this relay command can always execute
        /// </summary>
        /// <param name="parameter">doesnt matter</param>
        /// <returns>true</returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Executes the command (relay command doesn't take parameter)
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            action();
        }
    }
}
