using System;
using System.Collections.Generic;
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
using TaskManager.ViewModel;

namespace TaskManager.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //this.Top = Properties.Settings.Default.Top;
            //this.Left = Properties.Settings.Default.Left;
            this.Height = Properties.Settings.Default.Height;            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Properties.Settings.Default.Top = this.Top;
            //Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Save();
        }
    }
}
