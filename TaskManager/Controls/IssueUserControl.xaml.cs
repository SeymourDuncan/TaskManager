using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Command;

namespace TaskManager.Controls
{
    /// <summary>
    /// Логика взаимодействия для IssueUserControl.xaml
    /// </summary>
    public partial class IssueUserControl : UserControl
    {
        public IssueUserControl()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        public static readonly DependencyProperty CloseCommandProperty =
      DependencyProperty.Register(
           "CloseCommand",
           typeof(ICommand),
           typeof(IssueUserControl));

        public ICommand CloseCommand
        {
            get
            {
                return (ICommand)GetValue(CloseCommandProperty);
            }
            set
            {
                SetValue(CloseCommandProperty, value);
            }
        }

        public static readonly DependencyProperty CloseCommandParameterProperty =
        DependencyProperty.Register(
             "CloseCommandParameter",
             typeof(object),
             typeof(IssueUserControl));

        public object CloseCommandParameter
        {
            get
            {
                return (object)GetValue(CloseCommandParameterProperty);
            }
            set
            {
                SetValue(CloseCommandParameterProperty, value);
            }
        }

        public static readonly DependencyProperty IsActivePropery = DependencyProperty.Register("IsActive", typeof(bool), typeof(IssueUserControl), new PropertyMetadata(false));
        public bool IsActive{
            get
            {
                return (bool)GetValue(IsActivePropery);
            }
            set
            {
                SetValue(IsActivePropery, value);
            }
        }       

        public static readonly DependencyProperty IssueNamePropery = DependencyProperty.Register("IssueName", typeof(string), typeof(IssueUserControl), new PropertyMetadata(""));
        public string IssueName
        {
            get { return (String)GetValue(IssueNamePropery); }
            set { SetValue(IssueNamePropery, value); }
        }

        public static readonly DependencyProperty TimerPropery = DependencyProperty.Register("Timer", typeof(string), typeof(IssueUserControl), new PropertyMetadata("00:00:00"));
        public string Timer
        {
            get { return (String)GetValue(TimerPropery); }
            set { SetValue(TimerPropery, value); }
        }

        public static readonly DependencyProperty StartTimerCommandProperty = DependencyProperty.Register("StartTimerCommand", typeof(ICommand), typeof(IssueUserControl), new PropertyMetadata(null));
        public ICommand StartTimerCommand
        {
            get
            {
                return (ICommand)GetValue(StartTimerCommandProperty);
            }
            set
            {
                SetValue(StartTimerCommandProperty, value);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsActive = !IsActive;
        }






        //public static readonly DependencyProperty ParameterCommandProperty = DependencyProperty.Register("ParameterCommand", typeof(object), typeof(IssueUserControl), new PropertyMetadata(null));
        //public object ParameterCommand
        //{
        //    get { return (object)GetValue(ParameterCommandProperty); }
        //    set { SetValue(ParameterCommandProperty, value); }
        //}
    }

  
}
