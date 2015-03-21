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
using System.Windows.Forms;
using System.Drawing;
using Hardcodet.Wpf.TaskbarNotification;

namespace ScreenRecorder
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();
            
            //Minimize the window and don't put in the taskbar
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            ShowStandardBalloon();

            /*TaskbarIcon tbi = new TaskbarIcon();
            tbi.Icon = Properties.Resources.Error;
            tbi.ToolTipText = "hello world";*/
            
           
            
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            if(WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
                ShowInTaskbar = true;
                //hide balloon
                MyNotifyIcon.HideBalloonTip();
            }
            
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Minimized;
                ShowInTaskbar = false;
                ShowStandardBalloon();
                
            }
        }

        private void menuQuit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ShowStandardBalloon()
        {
            string title = "ScreenRecorder";
            string text = "L'application est minimisée";

            //show balloon with built-in icon
            MyNotifyIcon.ShowBalloonTip(title, text, BalloonIcon.Error);

            //show balloon with custom icon
            MyNotifyIcon.ShowBalloonTip(title, text, MyNotifyIcon.Icon);
        }

        

        

       

        
    }
}
