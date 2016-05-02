using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Redmine.Net.Api.Types;
using TaskManager.Model;
using TaskManager.View;

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
        private bool _isConnected;
        private string _loginButtonCaption = "Login";
        private ICommand _showIssuesSelectWindowCommand;
        
        private TrackedIssueList _trackedIssues;
        private ICommand _settingsCommand;
        private ICommand _startCommand;
        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();

        #endregion

        #region Properties

        // 
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                LoginButtonCaption = _isConnected ? "Logout" : "Login";
            }
        }

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
        

        private DateTime startTimePoint;
        public ICommand StartCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new RelayCommand(() =>
                {
                    if (_dispatcherTimer.IsEnabled)
                    {
                        _dispatcherTimer.Stop();
                        var span = _dispatcherTimer.Interval;                                                
                    }
                    else
                    {
                        startTimePoint = DateTime.Now;
                        _dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
                        _dispatcherTimer.Tick += _dispatcherTimer_Tick;
                        _dispatcherTimer.Start();
                    }
                    
                }));                               
            }
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            long tick = DateTime.Now.Ticks - startTimePoint.Ticks;
            DateTime stopWatch = new DateTime();
            stopWatch = stopWatch.AddTicks(tick);
            
        }

        #endregion

        #region Methods
        private void ShowAuthView()
        {
            // чистим пароль
            Properties.Settings.Default.Password = "";
            Messenger.Default.Send(new ShowDialogMessage(DialogWindowTypes.DwAuth));           
        }

        private void ShowIssuesSelectWindow()
        {
            Messenger.Default.Send(new ShowDialogMessage(DialogWindowTypes.DwIssueList));
            //TrackedIssues.SaveToXmlFile();            
            TrackedIssues.SaveToJsonFile();
        }

        public void Dc_OnStatusChange(object sender, ConnectionStatus status)
        {
            IsConnected = status == ConnectionStatus.ConnectionSuccessed;
            StatusText = ConstantsHelper.ConnectionStatusLegend[(int)status];
            UserName = Program.Instance.CurrentUser;
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
            // пробуем сходу законнектиться
            MainBusy = true;
            var connTask = Task.Factory.StartNew(() =>
            {
                IsConnected = program.Connect(Properties.Settings.Default.Login, Properties.Settings.Default.Password);
            }).ContinueWith((task) =>
            {
                MainBusy = false;            
                StatusText = IsConnected
                    ? ConstantsHelper.ConnectionStatusLegend[(int) ConnectionStatus.ConnectionSuccessed]
                    : ConstantsHelper.ConnectionStatusLegend[(int) ConnectionStatus.ConnectionFailed];

                // если не подключились, то вызываем форму
                if (!IsConnected)
                    ShowAuthView();
                else
                {
                    UserName = Program.Instance.CurrentUser;
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
         
            // гарантированно возвращает TrackedIssueList   
            TrackedIssues = new TrackedIssueList();
            TrackedIssues.LoadFromJsonFile();
        }

#endregion
    }

 
}
