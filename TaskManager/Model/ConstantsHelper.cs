using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Model
{
    public static class ConstantsHelper
    {     
        //public const string RedmineHost = "http://redmine.ufntc.ru";
        public static readonly string[] ConnectionStatusLegend = { "Connection failed", "Connection successed", "Logged out" };
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
