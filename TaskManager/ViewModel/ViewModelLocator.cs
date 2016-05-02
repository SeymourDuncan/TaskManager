using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using TaskManager.Model;
using TaskManager.View;
using System.Windows;

namespace TaskManager.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            // регистрируем ViewModel-и
            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register<AuthViewModel>();
            SimpleIoc.Default.Register<IssuesListViewModel>();
            // подписываемся на сообщения отрисовки модальных окон
            Messenger.Default.Register<ShowDialogMessage>(this, CreateDialogWindow);
        }

        public MainWindowViewModel MainViewModel
        {
            get { return ServiceLocator.Current.GetInstance<MainWindowViewModel>(); }
        }

        public AuthViewModel AuthViewModel
        {
            get { return ServiceLocator.Current.GetInstance<AuthViewModel>(); }
        }

        public IssuesListViewModel IssuesListViewModel
        {
            get { return ServiceLocator.Current.GetInstance<IssuesListViewModel>(); }
        }

        private void CreateDialogWindow(ShowDialogMessage msg)
        {
            switch (msg.Type)
            {
                // модальное окно авторизации
                case DialogWindowTypes.DwAuth:
                    var auth = new AuthDialog
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    AuthViewModel.Clear();
                    AuthViewModel.CloseDialogAction = new Action(auth.Close);
                    AuthViewModel.OnStatusChange += MainViewModel.Dc_OnStatusChange;
                    auth.ShowDialog();
                    break;
                // модальное окно выбора отслеживаемых задач
                case DialogWindowTypes.DwIssueList:
                    var issuesView = new IssuesList()
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    IssuesListViewModel.Clear();
                    IssuesListViewModel.AlreadyTrackedIssues = MainViewModel.TrackedIssues.Items;
                    IssuesListViewModel.SendTrackedIssues += MainViewModel.TrackedIssuesChanged;                    
                    IssuesListViewModel.CloseDialogAction = new Action(issuesView.Close);                    
                    issuesView.ShowDialog();
                    break;
            }
            
        }
    }
}
