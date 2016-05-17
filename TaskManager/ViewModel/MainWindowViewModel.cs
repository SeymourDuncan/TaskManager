using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Redmine.Net.Api.Types;
using TaskManager.Model;
using TaskManager.View;
using System.Globalization;

namespace TaskManager.ViewModel
{
    public class MainWindowViewModel:ViewModelBase
    {
        #region Fields
        private ICommand _authoriCommand;
        private ICommand _closeIssueCommand;
        private ICommand _loadedCommand;
        private ICommand _closeCommand;
        private string _statusText;
        private bool _mainBusy;
        private string _userName;
        private string _loginButtonCaption = "Login";
        private ICommand _showIssuesSelectWindowCommand;
        
        private TrackedIssueList _trackedIssues;
        private ICommand _settingsCommand;
        private IList<TimeEntryActivity> _timeEntryActivityList;

        #endregion

        #region Properties
        
        
        public string Title { get; set; } = "Task Manager";
        public bool MainBusy
        {
            get { return _mainBusy; }
            set
            {
                _mainBusy = value;
                RaisePropertyChanged(() => MainBusy);
            }
        }
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                RaisePropertyChanged(() => StatusText);
            }
        }

        public Program program => Program.Instance;

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                RaisePropertyChanged(() => UserName);
            }
        }

        public string LoginButtonCaption
        {
            get { return _loginButtonCaption; }
            set
            {
                _loginButtonCaption = value;
                RaisePropertyChanged(() => LoginButtonCaption);
            }
        }
        
        public TrackedIssueList TrackedIssues
        {
            get { return _trackedIssues; }
            set
            {
                _trackedIssues = value;
                RaisePropertyChanged(() => TrackedIssues);
            }
        }

        public IList<TimeEntryActivity> TimeEntryActivityList
        {
            get { return _timeEntryActivityList; }
            set
            {
                _timeEntryActivityList = value;
                RaisePropertyChanged(()=> TimeEntryActivityList);
            }
        }

        #endregion

        public void TrackedIssuesChanged(object sender, IList<Issue> issues)
        {
            // обновления списка отслеживаемых
            if (issues == null) return;
            foreach (var iss in issues)
            {
                var trIss = new TrackedIssue() {IssueItem = iss};
                TrackedIssues.AddItem(trIss);
            }           
        }
        #region Commands
        // после загрузки главного окна выполняем инициализацию модели
        public ICommand CloseIssueCommand
        {
            get
            {
                return _closeIssueCommand ?? (_closeIssueCommand = new RelayCommand<object>((obj) =>
                {
                    var trIs = (TrackedIssue) obj;
                    TrackedIssues.DeleteItem(trIs);
                }));
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                return _authoriCommand ?? (_authoriCommand = new RelayCommand(ShowAuthView));
            }
        }

        public ICommand ShowIssuesSelectWindowCommand
        {
            get { return _showIssuesSelectWindowCommand ?? (_showIssuesSelectWindowCommand = new RelayCommand(ShowIssuesSelectWindow)); }
        }

        // нужно сначала прогрузить главную форму, потому что Init может вызвать окно авторизации
        // а ему нужен родитель
        public ICommand LoadedCommand
        {
            get { return _loadedCommand?? (_loadedCommand = new RelayCommand(Init)); }
        }
        public ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(() =>
            {
                TrackedIssues?.SaveToJsonFile();
            })); }
        }

        #endregion

        #region Methods
        private void ShowAuthView()
        {                                    
            Messenger.Default.Send(new ShowDialogMessage(DialogWindowTypes.DwAuth));           
        }

        private void ShowIssuesSelectWindow()
        {
            Messenger.Default.Send(new ShowDialogMessage(DialogWindowTypes.DwIssueList));            
            TrackedIssues.SaveToJsonFile();
        }

        public void Dc_OnStatusChange(object sender, LogMessage status)
        {            
            StatusText = status.Head;
            UserName = Program.Instance.CurrentUser;
            // пробуем поднять активитиз
            TimeEntryActivityList = Program.Instance.GetTimeEntryActivityList();
        }

        public ICommand SettingsCommand
        {
            get { return _settingsCommand ?? (_settingsCommand = new RelayCommand(
                () =>
                {
                    TrackedIssues.ActiveIssueToFirstPosition();
                }
                )); }
        }

        public void Init()
        {
            // подписываемся на оповещалку
           Program.Instance.ShowLogMessage += Dc_OnStatusChange;
           bool isConnected = false;
            // пробуем сходу законнектиться
           MainBusy = true;
           Task.Factory.StartNew(() =>
           {
               isConnected = program.Connect(Properties.Settings.Default.Host, Properties.Settings.Default.Apikey);
            }).ContinueWith((task) =>
            {
                MainBusy = false;                            

                // если не подключились, то вызываем форму
                if (!isConnected)
                    ShowAuthView();
                else
                {
                    UserName = Program.Instance.CurrentUser;
                }
                // загружаем справочник действий
                TimeEntryActivityList = Program.Instance.GetTimeEntryActivityList();

                // загружаем задачи         
                TrackedIssues = new TrackedIssueList();
                TrackedIssues.LoadFromJsonFile();
            }, TaskScheduler.FromCurrentSynchronizationContext());                               
        }

#endregion
    }
   
}
