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
        private string _host;
        private string _apikey;

        public bool Connection { get; set; } = false;

        public string Host
        {
            get { return _host; }
            set
            {
                _host = value;
                RaisePropertyChanged(()=> Host);
            }
        }
        public string ApiKey
        {
            get { return _apikey; }
            set
            {
                _apikey = value;
                RaisePropertyChanged(() => ApiKey);
            }
        }


        public void Clear()
        {
            AuthErrorText = "";              
            Host = Properties.Settings.Default.Host;
            ApiKey = Properties.Settings.Default.Apikey;
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

        public ICommand SubmitRegCommand => _submitRegCommand?? (_submitRegCommand = new RelayCommand(() =>
        {
            Busy = true;
            
            var connTask = Task.Factory.StartNew(() =>
            {
                Connection = Program.Instance.Connect(Host, ApiKey);
            }).ContinueWith((task) =>
            {
                Busy = false;
                if (Connection)
                {
                    // успех - закрываем диалог
                    CloseDialog();
                }
                AuthErrorText = "Connection falied";
            }, TaskScheduler.FromCurrentSynchronizationContext());
                              
        },
            () =>
            {                
                if (string.Equals(string.Empty, Host) || string.Equals(string.Empty, ApiKey))
                    return false;
                return true;
            }));
        

        private void CloseDialog()
        {               
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
                    Clear();
                }));
            }
        }

        public Action CloseDialogAction { get; set; }
        public ICommand CloseCommand => _closeCommand??(_closeCommand = new RelayCommand(CloseDialog));
    }
}
