using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using AForge.Video;
using AForge.Video.FFMPEG;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace ScreenRecorder
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly VideoFileWriter writer;
        private bool rec = false;

        private Rectangle screenSize = Screen.PrimaryScreen.Bounds;

        private UInt32 frameCount = 0;

        private readonly int width = SystemInformation.VirtualScreen.Width;
        private readonly int height = SystemInformation.VirtualScreen.Height;

        private ScreenCaptureStream streamVideo;

        readonly Stopwatch stopwatch = new Stopwatch();

        private string _currentOpenedFile;

        public MainWindow()
        {
            InitializeComponent();

            MouseDown += MainWindow_MouseDown;
            
            //Minimize the window and don't put in the taskbar
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            ShowStandardBalloon();

            writer = new VideoFileWriter();
        }

        /// <summary>
        /// Can move the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
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
            //Save the video screen in a file
            rec = false;
            try
            {
                rec = false;

                Console.WriteLine(@"FILE SAVED !!!!");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            //Quit the application
            Application.Current.Shutdown();
        }

        
        private void ShowStandardBalloon()
        {
            const string title = "ScreenRecorder";
            const string text = "L'application est minimisée";

            //show balloon with built-in icon
            MyNotifyIcon.ShowBalloonTip(title, text, BalloonIcon.Error);

            //show balloon with custom icon
            MyNotifyIcon.ShowBalloonTip(title, text, MyNotifyIcon.Icon);
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"Recording has started");
            if (rec == false)
            {
                rec = true;

                frameCount = 0;

                var time = DateTime.Now.ToString("yy-mmm-dd ddd");
                var compName = Environment.UserName;
                var fullName = "c:\\ScreenRecorder" + "\\" + compName.ToUpper() + "_" + time;

                try
                {
                    //Change FPS and the codec for video
                    writer.Open(fullName + ".avi",
                        width,
                        height,
                        20,
                        VideoCodec.MPEG4);
                }
                catch (Exception exception)
                {

                    Console.WriteLine(exception.Message);
                }
                //Start the main process to capture
                Process();
            }
        }

        private void Process()
        {
            try
            {
                var screenArea = Rectangle.Empty;
                foreach (var screen in Screen.AllScreens)
                {
                    screenArea = Rectangle.Union(screenArea, screen.Bounds);
                }

                streamVideo = new ScreenCaptureStream(screenArea);

                streamVideo.NewFrame += new NewFrameEventHandler(video_NewFrame);

                streamVideo.Start();

                stopwatch.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (rec)
                {
                    writer.WriteVideoFrame(eventArgs.Frame);
                }

                else
                {
                    stopwatch.Reset();
                    Thread.Sleep(500);
                    streamVideo.SignalToStop();
                    Thread.Sleep(500);
                    writer.Close();
                }
            }
            catch (Exception exception)
            {

                Console.WriteLine(exception.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        

        

       

        
    }
}
