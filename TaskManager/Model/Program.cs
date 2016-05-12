using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using System.Web.Script.Serialization;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using Redmine.Net.Api.JSonConverters;

namespace TaskManager.Model
{
    public class Program
    {
        private static Program _instance;
        public static Program Instance => _instance ?? (_instance = new Program());
        public string CurrentUser { get; set; } = "";
        public int Offset { get; set; } = 0;
        private readonly int issuesLimit = 10;
        #region Properties
        public RedmineManager RedmineMng { get; set; }
        #endregion

        #region Methods

        public bool Connect(string login, string password)
        {
            CurrentUser = "";
            RedmineMng = new RedmineManager(ConstantsHelper.RedmineHost, login, password);
            // to check auth
            try
            {
                var user = RedmineMng.GetCurrentUser();
                CurrentUser = $"{user.LastName} {user.FirstName}";
            }
            catch (Exception e)
            {
                return false;
            }

            // сохраняем удачное подключение
            Properties.Settings.Default.Login = login;
            Properties.Settings.Default.Password = password;
            Properties.Settings.Default.Save();

            return true;
        }

        public IList<Issue> GetIssuesList()
        {
            try
            {
                var parameters = new NameValueCollection
                { 
                    { "status_id", "open" },
                    { "assigned_to_id", "me" },
                    { "offset", Offset.ToString() },
                    { "limit", issuesLimit.ToString() }
                };
                var issues = RedmineMng.GetObjectList<Issue>(parameters);                
                Offset += issuesLimit;               
                return issues;                
            }
            catch (Exception e)
            {
                return null;               
            }
        }
        #endregion

        public void SaveTrackedIssuesToFile(TrackedIssueList issues)
        {
            //var path = Path.Combine(Environment.CurrentDirectory, "Data.xml");            
            var path = Environment.CurrentDirectory + "\\Data.xml";
            //var co = new CSomeObjects();
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(TrackedIssueList));
                using (var writer = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(writer, issues);                    
                }
            }
            catch (Exception e)
            {
                return;
            }           
        }

        public TrackedIssueList LoadTrackedIssuesFromFile()
        {
            //var path = Path.Combine(Environment.CurrentDirectory, "Data.xml");
            var path = Environment.CurrentDirectory + "\\Data.xml";
            try
            {                                
                using (XmlReader xmlReader = new XmlTextReader(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TrackedIssueList));
                    return (TrackedIssueList)serializer.Deserialize(xmlReader);                    
                }
            }
            catch (Exception e)
            {
                return new TrackedIssueList();
            }            
        }
        #region Events

        #endregion
    }

   
        
    public class TrackedIssue: ObservableObject
    {
        private bool _isActive;
        private long _trackedTime;
        public Issue IssueItem { get; set; }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                // если делаем активным, посылаем сообщение чтобы деактивировать последний
                if (_isActive)
                {
                    OnIsActiveChanged();
                }               
                // если деактивируем задачу, останавливаем таймер     
                else
                {
                    IsRunning = false;
                }
                RaisePropertyChanged();                
            }
        }

        [ScriptIgnore]
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                if (_isRunning)
                {
                    StartTimer();
                }
                else
                {
                    StopTimer();
                }
                RaisePropertyChanged();
            }
        }

        public long TrackedTime
        {
            get
            {
                return _trackedTime;                
            }
            set
            {
                _trackedTime = value;
                RaisePropertyChanged();
            }
        }

        public event EventHandler IsActiveChanged;

        private void OnIsActiveChanged()
        {
            var e = IsActiveChanged;
            e?.Invoke(this, null);
        }

        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private DateTime _startTimePoint;
        private long _accumActiveTicks;
        
        private bool _isRunning;
        private ICommand _commitCommand;

        private void StopTimer()
        {
            _dispatcherTimer.Stop();
        }
        public void StartTimer()
        {
            // запомним сколько времени уже было у задачи
            _accumActiveTicks = TrackedTime;
            _startTimePoint = DateTime.Now;
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(1);                        
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var tick = DateTime.Now.Ticks - _startTimePoint.Ticks;
            TrackedTime = tick + _accumActiveTicks;
        }

        private void CommitIssue(CommitParameters comParams)
        {            
            var dt = new DateTime(_trackedTime);
            var zeroDate = new DateTime();
            var qwe = dt - zeroDate;            
            string hours = $"{qwe.TotalHours:0.##}";

            var acts = Program.Instance.RedmineMng.GetObjectList<TimeEntryActivity>(new NameValueCollection() {});
            try
            {
                var timeEntry = new TimeEntry()
                {
                    Issue = new IdentifiableName() {Id = IssueItem.Id},
                    Activity = new IdentifiableName() {Id = 9}, // пока Программирование
                    Comments = comParams.Comment,
                    CreatedOn = DateTime.Now,
                    CustomFields = null,
                    Hours = (decimal) qwe.TotalHours
                };

                Program.Instance.RedmineMng.CreateObject(timeEntry);
            }
            catch (Exception e)
            {
                return;
            }
            

        }
        public ICommand CommitCommand
        {
            get
            {
                return _commitCommand ?? (_commitCommand = new RelayCommand<object>((obj) =>
                {
                    if (!(obj is CommitParameters))
                        return;
                    CommitIssue((CommitParameters) obj);
                }));
            }            
        }
    }

    public class TrackedIssueList: ObservableObject
    {
        private ObservableCollection<TrackedIssue> _items;
        private readonly string _path = Environment.CurrentDirectory + "\\Data.json";
        
        public ObservableCollection<TrackedIssue> Items
        {
            get { return _items; }
            set
            {
                _items = value;                
            }
        }

        public TrackedIssue ActiveIssue { get; set; }        

        public TrackedIssueList()
        {
            Items = new ObservableCollection<TrackedIssue>();
        }
        
        public void AddItem(TrackedIssue iss)
        {
            if (iss == null)
                return;

            Items.Add(iss);            
            if (iss.IsActive)
                ActiveIssue = iss;
            iss.IsActiveChanged += ActiveIssueChanged;
        }

        public void AddItems(IList<TrackedIssue> issues)
        {
            if (issues == null)
                return;
                foreach (var iss in issues)
            {
                AddItem(iss);
            }            
        }

        public void DeleteItem(TrackedIssue issue)
        {
            bool itWasActive = ActiveIssue == issue;
                Items.Remove(issue);
            if (itWasActive && Items.Count > 0)
            {
                var firstIss = Items[0];
                if (firstIss != null)
                    firstIss.IsActive = true;
            }
        }
        private void ActiveIssueChanged(object sender, EventArgs args)
        {
            // активной может быть только одна задача,
            // поэтому каждый раз выставляя задаче статус "активная", снимем "активность" с предыдущей
            // TODO список задач слушает изменения каждой задачи, что не есть хорошо. Как сделать красиво???
            
            var iss = (TrackedIssue) sender;
            if (ActiveIssue != null)
                ActiveIssue.IsActive = false;            
            ActiveIssue = iss;
            // BUG хочу сортировать задачи, чтоб активная перестасивалась в начало списка, но ловлю багу
            //ActiveIssueToFirstPosition();
        }        

        public void ActiveIssueToFirstPosition()
        {
            if (Items.Count < 1)
                return;
            if (ActiveIssue == null)
                return;
            var actIdx = Items.IndexOf(ActiveIssue);
            Items.Swap(0, actIdx);
        }

        //private TrackedIssue GetTrackedIssueById(int id)
        //{
        //    return Items.FirstOrDefault((trIss) =>
        //    {
        //        return trIss.IssueItem.Id == id;
        //    });
        //}


        public void SaveToJsonFile()
        {
            var serializer = new JavaScriptSerializer();
            try
            {
                var serializedResult = serializer.Serialize(Items);
                File.WriteAllText(_path, serializedResult);
            }
            catch (Exception e)
            {

            }
        }

        public void LoadFromJsonFile()
        {
            var serializer = new JavaScriptSerializer();
            try
            {
                string json = File.ReadAllText(_path);
                IList<TrackedIssue> list = serializer.Deserialize<ObservableCollection<TrackedIssue>>(json);
                AddItems(list);
            }
            catch (Exception e)
            {

            }
        }
    }

    public static class ListUtils
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            var col = new ObservableCollection<T>();
            foreach (var cur in enumerable)
            {
                col.Add(cur);
            }
            return col;
        }

        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }

    public class CommitParameters
    {
        public string Comment { get; set; }
        public bool SolveIssue { get; set; }
    }
}
