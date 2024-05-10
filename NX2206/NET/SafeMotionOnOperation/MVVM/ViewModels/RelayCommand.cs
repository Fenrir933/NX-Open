using System;
using System.Windows.Input;

namespace SafeMotionOnOperation.MVVM.ViewModels {
    public class RelayCommand<T> : ICommand {

        #region Fields

        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        #endregion

        /// <summary>
        /// Initialisiert ein neues RelayCommand Objekt, welches immer ausgeführt werden kann.
        /// </summary>
        /// <param name="execute">Funktionszeiger auf Ausführungslogik</param>
        /// <exception cref="ArgumentNullException">Wenn Funktionszeiger null ist.</exception>
        public RelayCommand(Action<T> execute)
            : this(execute, null) {
        }

        /// <summary>
        /// Initialisiert ein neues RelayCommand Objekt.
        /// </summary>
        /// <param name="execute">Funktionszeiger auf Ausführungslogik.</param>
        /// <param name="canExecute">Funktionszeiger auf Prüflogik.</param>
        /// <exception cref="ArgumentNullException">Wenn Funktionszeiger null ist.</exception>
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
