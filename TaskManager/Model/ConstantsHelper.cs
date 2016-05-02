using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Model
{
    public static class ConstantsHelper
    {

        //public const string HostName = "db4free.net";
        //public const string Port = "3306";
        //public const string User = "sladkomax";
        //public const string Password = "biberisthebest";
        //public const string DBName = "sladkomax";

        //public const string ConnectionString =
        //    "server=" + HostName
        //    + ";port=" + Port
        //    + ";uid=" + User
        //    + ";pwd=" + Password
        //    + ";database=" + DBName + ";";

        public const string RedmineHost = "http://redmine.ufntc.ru";
        public static readonly string[] ConnectionStatusLegend = { "Connection failed", "Connection successed", "Logged out" };
    }

    public static class Queries
    {
        public const string Issues = "/issues.json?status_id=open&assigned_to_id=me";
    }

    public enum ConnectionStatus
    {
        ConnectionFailed, ConnectionSuccessed, Logout
    } ;

    // диалоговые окна
    public enum DialogWindowTypes
    {
        DwAuth = 0,
        DwIssueList = 1
    }

    public class ShowDialogMessage
    {
        public ShowDialogMessage(DialogWindowTypes type)
        {
            Type = type;
        } 
        public DialogWindowTypes Type { get; set; }
    }
}
