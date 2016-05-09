using System;
using System.Collections.Generic;
using System.Globalization;
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
using TaskManager.Model;

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
       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TrackedIssue.IsActive = !TrackedIssue.IsActive;
        }       

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TrackedIssue.IsRunning = !TrackedIssue.IsRunning;
        }               

        public static readonly DependencyProperty TrackedIssuePropery = DependencyProperty.Register("TrackedIssue", typeof(TrackedIssue), typeof(IssueUserControl), new PropertyMetadata(null));
        public TrackedIssue TrackedIssue
        {
            get { return (TrackedIssue)GetValue(TrackedIssuePropery); }
            set { SetValue(TrackedIssuePropery, value); }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            AdditionalInfo.Visibility = Visibility.Collapsed;
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            AdditionalInfo.Visibility = Visibility.Visible;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertBoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool booleanValue = (bool)value;            
            return booleanValue? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool booleanValue = (bool)value;
            return booleanValue ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(long), typeof(DateTime))]
    public class TicksToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ticks = (long)value;
            return new DateTime(ticks);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
