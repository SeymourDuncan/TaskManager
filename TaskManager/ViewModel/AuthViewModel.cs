using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using TaskManager.Model;

namespace TaskManager.ViewModel
{
    public class AuthViewModel: ViewModelBase
    {
        private ICommand _closeCommand;
        private ICommand _submitRegCommand;
        private string _authErrorText;
        private bool _busy;
        private string _login;

        public ConnectionStatus Connection { get; set; } = ConnectionStatus.Logout;

        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                RaisePropertyChanged(()=>Login);
            }
        }


        public void Clear()
        {
            AuthErrorText = "";
            // чистим подписки
            OnStatusChange = null;
            Login = Properties.Settings.Default.Login;            
        }
        public bool Busy
        {
            get { return _busy; }
            set
            {
                _busy = value; 
                RaisePropertyChanged(() => Busy);
            }
        }

        public string AuthErrorText
        {
            get { return _authErrorText; }
            set
            {
                _authErrorText = value;
                RaisePropertyChanged(() => AuthErrorText);
            }
        }

        public ICommand SubmitRegCommand => _submitRegCommand?? (_submitRegCommand = new RelayCommand<object>((param) =>
        {
            var password = ((PasswordBox)param) ?.Password;

            Busy = true;
            
            var connTask = Task.Factory.StartNew(() =>
            {
                Connection = Program.Instance.Connect(Login, password) ? ConnectionStatus.ConnectionSuccessed : ConnectionStatus.ConnectionFailed;
            }).ContinueWith((task) =>
            {
                Busy = false;
                if (Connection == ConnectionStatus.ConnectionSuccessed)
                {
                    // успех - закрываем диалог
                    CloseDialog();
                }
                AuthErrorText = "Wrong user name or password";
            }, TaskScheduler.FromCurrentSynchronizationContext());
                              
        },
            (param) =>
            {
                if (param == null)
                    return false;

                var password = (PasswordBox) param;
                if (string.Equals(string.Empty, password.Password) || string.Equals(string.Empty, Login))
                    return false;
                return true;
            }));

        public event EventHandler<ConnectionStatus> OnStatusChange;

        private void CloseDialog()
        {   
            // закрывая диалог сообщаем подписчикам о состоянии подключения        
            OnStatusChange?.Invoke(this, Connection);
            Clear();
            CloseDialogAction();
        }

        private ICommand _closeByCrossCommand;
        public ICommand CloseByCroosCommand
        {
            get
            {
                return _closeByCrossCommand ?? (_closeByCrossCommand = new RelayCommand(() =>
                {
                    OnStatusChange?.Invoke(this, Connection);
                    Clear();
                }));
            }
        }

        public Action CloseDialogAction { get; set; }
        public ICommand CloseCommand => _closeCommand??(_closeCommand = new RelayCommand(CloseDialog));
    }
}
