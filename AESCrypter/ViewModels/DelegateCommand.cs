using System;
using System.Windows.Input;

namespace AESCrypter.ViewModels
{

    /// <summary>
    /// 委托命令类，所有的命令都通过委托绑定相关的执行函数来生成。
    /// </summary>
    class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        //这里通过系统自带的CommandManager实现
        //每当CommandManager认为有些事情发生了变化，会影响命令执行的能力时，
        //就会引发CommandManager.RequerySuggested事件。
        //https://stackoverflow.com/a/31807391
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }


        public DelegateCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            if (canExecute == null) _canExecute = (obj => true);
            else _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

    }

}