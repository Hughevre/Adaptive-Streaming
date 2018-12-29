using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AdaptiveStreaming
{
    class RelayCommand<T> : ICommand where T : class
    {
        #region Properties
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;
        #endregion //Properties

        #region Constructors

        public RelayCommand(Action<T> _execute)
            : this(_execute, null)
        {
        }

        public RelayCommand(Action<T> _execute, Predicate<T> _canExecute)
        {
            execute = _execute ?? throw new ArgumentNullException("execute");
            canExecute = _canExecute;
        }
        #endregion // Constructors

        #region ICommand Implementation

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            execute((T)parameter);
        }
        #endregion // ICommand Implementation
    }
}
