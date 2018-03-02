using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Larawag.Utils.Commands
{
    //https://gist.github.com/schuster-rainer/2648922

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action execute)
            : this(execute, null) { }

        public RelayCommand(Action execute, Func<bool> canExecute)
            : base(param => execute(), param => { return canExecute != null ? canExecute() : true; }) { }
    }

    public class RelayCommand<T> : ICommand  where T : class
    {
        Action<T> action; 
        Func<T, bool> canExecute;

        protected RelayCommand()
        {

        }

        public RelayCommand(Action<T> action) : this (action, null)
        {
        }

        public RelayCommand(Action<T> action, Func<T, bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        #region ICommand Members
        public bool CanExecute(object parameter)
        {
            if (canExecute != null)
            {
                return canExecute(parameter as T);
            }
            else
            {
                return true;
            }
        }

        public event EventHandler CanExecuteChanged
        { 
            // wire the CanExecutedChanged event only if the canExecute func
            // is defined (that improves perf when canExecute is not used)
            add
            {
                if (this.canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {

                if (this.canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public virtual void Execute(object parameter)
        {
            if (parameter != null)
            {
                action(parameter as T);
            }
            else
            {
                action(null);
            }
        }

        #endregion
    }
}
