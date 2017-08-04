using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFHexaEditorExample.Commands {
    public class DelegateCommand<T> : ICommand {
        private readonly Action<T> executeMethod = null;
        private readonly Func<T, bool> canExecuteMethod = null;

        public DelegateCommand(Action<T> executeMethod)
        : this(executeMethod, null) { }

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod) {
            if (executeMethod == null)
                throw new ArgumentNullException("executeMetnod");
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        #region ICommand 成员
        /// <summary>
             ///  Method to determine if the command can be executed
             /// </summary>
        public bool CanExecute(T parameter) {
            if (canExecuteMethod != null) {
                return canExecuteMethod(parameter);
            }
            return true;

        }

        /// <summary>
             ///  Execution of the command
             /// </summary>
        public void Execute(T parameter) {
            if (executeMethod != null) {
                executeMethod(parameter);
            }
        }

        #endregion


        event EventHandler ICommand.CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #region ICommand 成员

        public bool CanExecute(object parameter) {
            if (parameter == null && typeof(T).IsValueType) {
                return (canExecuteMethod == null);

            }
            try {
                if (parameter.ToString() == "") {
                    return true;
                }
            }
            catch {
                //EventLogger.Logger.WriteLine("CanExecute出错:" + ex.Message + ex.Source + this.executeMethod.Method.Name);
                return false;
            }
            return CanExecute((T)parameter);
        }

        public void Execute(object parameter) {
            Execute((T)parameter);
        }

        #endregion
    }
}   
