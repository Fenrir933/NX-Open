using System;
using System.Windows.Input;

namespace MCSNumbers.MVVM.ViewModels {
    public class RelayCommand<T> : ICommand {

        #region Fields

        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        #endregion

        /// <summary>
        /// Initializes a new instance of the RelayCommand class that 
        /// can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
        public RelayCommand(Action<T> execute)
            : this(execute, null) {
        }

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute) {
            _execute = execute ?? throw new ArgumentNullException("execute");

            if (canExecute != null)
                _canExecute = canExecute;
        }

        #region ICommand Members

        public bool CanExecute(object parameter) =>
            _canExecute == null || _canExecute((T)parameter);

        public void Execute(object parameter) {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #endregion
    }
}
