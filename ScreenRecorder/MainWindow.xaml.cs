using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.FFMPEG;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;

namespace ScreenRecorder
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private VideoFileWriter writer;
        private bool rec = false;

        private Rectangle screenSize = Screen.PrimaryScreen.Bounds;

        private UInt32 frameCount = 0;

        private int width = SystemInformation.VirtualScreen.Width;
        private int height = SystemInformation.VirtualScreen.Height;

        private ScreenCaptureStream streamVideo;

        Stopwatch stopwatch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();

            
            
            //Minimize the window and don't put in the taskbar
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            ShowStandardBalloon();

            writer = new VideoFileWriter();
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
            rec = false;
            Application.Current.Shutdown();
        }

        
        private void ShowStandardBalloon()
        {
            var title = "ScreenRecorder";
            var text = "L'application est minimisée";

            //show balloon with built-in icon
            MyNotifyIcon.ShowBalloonTip(title, text, BalloonIcon.Error);

            //show balloon with custom icon
            MyNotifyIcon.ShowBalloonTip(title, text, MyNotifyIcon.Icon);
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (rec == false)
            {
                rec = true;

                frameCount = 0;

                string time = DateTime.Now.ToString("yy-mmm-dd ddd");
                string compName = Environment.UserName;
                string fullName = compName.ToUpper() + "_" + time;

                try
                {
                    writer.Open(fullName + ".avi",
                        width,
                        height,
                        25,
                        VideoCodec.MPEG4);
                }
                catch (Exception exception)
                {

                    Console.WriteLine(exception.Message);
                }
                //Start the main process to capture
                process();
            }
        }

        private void process()
        {
            try
            {
                Rectangle screenArea = Rectangle.Empty;
                foreach (Screen screen in Screen.AllScreens)
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
                    frameCount++;
                    writer.WriteVideoFrame(eventArgs.Frame);
                    Console.WriteLine("Frames: " + frameCount.ToString());
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
            try
            {
                rec = false;

                Console.WriteLine("FILE SAVED !!!!");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        

        

       

        
    }
}
