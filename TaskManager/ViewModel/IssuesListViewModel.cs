using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Redmine.Net.Api.Types;
using TaskManager.Model;

namespace TaskManager.ViewModel
{
    public class IssuesListViewModel: ViewModelBase
    {
        private ICommand _loadMoreCommand;
        private ObservableCollection<Issue> _issues;
        private ICommand _trackSelectedCommand;
        private ICommand _addCheckedIssueCommand;
        private ICommand _deleteUncheckedIssueCommand;
        private ICommand _loadedCommand;
        private bool _mainBusy;


        // это ссылка на TrackedIssues из MainViewModel!
        public IList<TrackedIssue> AlreadyTrackedIssues { get; set; }

        private ObservableCollection<Issue> SelectedIssues { get; set; }
        public ObservableCollection<Issue> Issues
        {
            get { return _issues; }
            set
            {
                _issues = value;
                RaisePropertyChanged(() => Issues);
            }
        }

        private void FilterIssuesList()
        {
            if (AlreadyTrackedIssues == null) return;
            // TODO некрасиво, но пусть пока так
            // выявим список повторяющихся
            List<Issue> isslist = Issues.Where(iss =>
            {
                return AlreadyTrackedIssues.FirstOrDefault(_iss => iss.Id == _iss.IssueItem.Id) != null;
            }).ToList();

            isslist.ForEach((iss) => { Issues.Remove(iss); });
        }

        public IssuesListViewModel()
        {            
            SelectedIssues = new ObservableCollection<Issue>();
        }

        public event EventHandler<IList<Issue>> SendTrackedIssues;

        private void OnSendTrackedIssues()
        {
            var e = SendTrackedIssues;
            e?.Invoke(this, SelectedIssues);
            var list = SelectedIssues.ToList();
            SelectedIssues.Clear();
            // список отслеживаемых изменился, заново отфильтруем
            FilterIssuesList();
        }
        public void Clear()
        {
            // чистим подписки
            SendTrackedIssues = null;         
            // чистим выбранные
            SelectedIssues.Clear();
        }

        private void CloseDialog()
        {
            Clear();
            CloseDialogAction();
        }

        public Action CloseDialogAction { get; set; }

        public ICommand LoadedCommand
        {
            get { return _loadedCommand ?? (_loadedCommand = new RelayCommand(LoadMore)); }
        }

        public ICommand AddCheckedIssueCommand
        {
            get { return _addCheckedIssueCommand?? (_addCheckedIssueCommand = new RelayCommand<Issue>(
                (issue) =>
                {
                    SelectedIssues.Add(issue);
                }
                )); }
        }

        public ICommand DeleteUncheckedIssueCommand
        {
            get
            {
                return _deleteUncheckedIssueCommand ?? (_deleteUncheckedIssueCommand = new RelayCommand<Issue>(

                    (issue) =>
                    {
                        SelectedIssues.Remove(issue);                        
                    }));
            }
        }

        public ICommand TrackSelectedCommand
        {
            get
            {
                return _trackSelectedCommand ?? (_trackSelectedCommand = new RelayCommand(OnSendTrackedIssues));
            }
        }

        public ICommand LoadMoreCommand
        {
            get
            {
                return _loadMoreCommand ?? (_loadMoreCommand = new RelayCommand(LoadMore));
                
            }
        }

        public bool MainBusy
        {
            get { return _mainBusy; }
            set
            {
                _mainBusy = value;
                RaisePropertyChanged(() => MainBusy);
            }
        }
        private void LoadMore()
        {            
            MainBusy = true;            
            var tsk = Task.Factory.StartNew<IList<Issue>>(() =>
            {
                var issues = Program.Instance.GetIssuesList();
                return issues;
            }).ContinueWith((task) =>
            {
                MainBusy = false;
                var issues = task.Result;
                if (issues == null)
                    return;            
                if (Issues == null)
                    Issues = issues.ToObservableCollection();
                else
                {
                    issues.ToList().ForEach(iss => Issues.Add(iss));
                }
                // уберем те, которые уже отслеживаются
                FilterIssuesList();
                
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }       
        
    }
}
