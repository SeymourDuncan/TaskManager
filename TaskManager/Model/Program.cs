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
        public Issue IssueItem { get; set; }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (_isActive)
                    OnIsActiveChanged();
                RaisePropertyChanged();                
            }
        }

        public DateTime TrackedTime { get; set; }        

        public event EventHandler IsActiveChanged;

        private void OnIsActiveChanged()
        {
            var e = IsActiveChanged;
            e?.Invoke(this, null);
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
            var iss = (TrackedIssue) sender;
            if (ActiveIssue != null)
                ActiveIssue.IsActive = false;            
            ActiveIssue = iss;
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
}
