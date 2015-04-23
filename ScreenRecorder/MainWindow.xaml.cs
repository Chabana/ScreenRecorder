﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using AForge.Video;
using AForge.Video.FFMPEG;
using AForge.Video.DirectShow;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;
using System.Security.Permissions;
using System.Windows.Controls;
using Image = System.Drawing.Image;

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

        private string snapShot = "";
        private string snapDir = "";
        public int waitLimit = 4000; //ms

        

        public MainWindow()
        {
            InitializeComponent();

            MouseDown += MainWindow_MouseDown;

            Folder folder = new Folder();
            var observableCollection = folder.Files;

            if (observableCollection == null) throw new ArgumentNullException(@"observableCollection");
            Console.WriteLine(observableCollection.Count);

            foreach (var fileInfo in observableCollection)
            {
                AddListLine(fileInfo.Name);
            }


            //Minimize the window and don't put in the taskbar
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            const string title = "ScreenRecorder";
            const string text = "L'application est minimisée";
            ShowStandardBalloon(title, text);

            writer = new VideoFileWriter();

            Update();
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
                const string title = "ScreenRecorder";
                const string text = "L'application est minimisée";
                ShowStandardBalloon(title, text);
                
            }
        }

        private void menuQuit_Click(object sender, RoutedEventArgs e)
        {
            //Save the video screen in a file
            stopVideoRecording();

            //Quit the application
            Application.Current.Shutdown();
        }

        
        private void ShowStandardBalloon(string title, string text)
        {
            //show balloon with built-in icon
            MyNotifyIcon.ShowBalloonTip(title, text, BalloonIcon.Error);

            //show balloon with custom icon
            MyNotifyIcon.ShowBalloonTip(title, text, MyNotifyIcon.Icon);
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


        /// <summary>
        /// Fonction pour prendre des screenshots
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuScreenshot_Click(object sender, RoutedEventArgs e)
        {
            Rectangle screenArea = Rectangle.Empty;

            foreach (Screen screen in Screen.AllScreens)
            {
                screenArea = Rectangle.Union(screenArea, screen.Bounds);
            }

            ScreenCaptureStream stream = new ScreenCaptureStream(screenArea);
            
            Thread.Sleep(1800);

            stream.NewFrame += new NewFrameEventHandler(dev_NewFrame);

            stream.Start();

            Console.WriteLine("Screenshot taken");
           
            
        }

        private void dev_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap bitmap = eventArgs.Frame.Clone() as Bitmap;
                ((ScreenCaptureStream)sender).SignalToStop();
                Image img = (Image) bitmap;
                string dirName = "c:\\ScreenRecorder";
                string flName = "videoCapture" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".png";
                flName = Path.Combine(dirName, flName);
                img.Save(flName, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            

        }

        private void stopVideoRecording()
        {
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
        }

        private void menuCaptureVideo_Click(object sender, RoutedEventArgs e)
        {
            if ((string) menuCaptureVideo.Header == "Start Video Capture")
            {
                var title = "Video has started recording";
                var text = "Don't forget to stop the video recording";
                menuCaptureVideo.Header = "Stop Video Capture";
                ShowStandardBalloon(title, text);

                
                if (rec == false)
                {
                    Console.WriteLine(@"Recording has started");
                    rec = true;

                    frameCount = 0;

                    var time = DateTime.Now.ToString("d_MMM_yyyy_HH_mm_ssff");
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
            else
            {
                var title = "Video has stopped recording";
                var text = "You are not being recorded";
                ShowStandardBalloon(title, text);
                stopVideoRecording();
                menuCaptureVideo.Header = "Start Video Capture";
                Console.WriteLine(@"Recording has stopped");
            }
            
        }

        //##########################################################################################################################
        //######################################## Part for the files update (Create, Delete, Rename) ##############################

        private void Update()
        {
            string folderPath = "c:\\ScreenRecorder";

            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            FileSystemWatcher watcher = new FileSystemWatcher();

            watcher.Path = folderPath;

            watcher.NotifyFilter = NotifyFilters.LastAccess |  NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Filter = "*.avi";

            watcher.Created += new FileSystemEventHandler(fileSystemWatcher_Created);
            watcher.Changed += new FileSystemEventHandler(fileSystemWatcher_Changed);
            watcher.Deleted += new FileSystemEventHandler(fileSystemWatcher_Deleted);
            watcher.Renamed += new RenamedEventHandler(fileSystemWatcher_Renamed);

            watcher.EnableRaisingEvents = true;
        }

        void fileSystemWatcher_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            DisplayFileSystemWatcherInfo(e.ChangeType, e.Name);
        }

        void fileSystemWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            DisplayFileSystemWatcherInfo(e.ChangeType, e.Name);
        }

        void fileSystemWatcher_Deleted(object sender, System.IO.FileSystemEventArgs e)
        {
            DisplayFileSystemWatcherInfo(e.ChangeType, e.Name);
        }

        void fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            DisplayFileSystemWatcherInfo(e.ChangeType, e.Name, e.OldName);
        }

        void DisplayFileSystemWatcherInfo(WatcherChangeTypes watcherChangeTypes, string name, string oldName = null)
        {
            if (watcherChangeTypes == WatcherChangeTypes.Renamed)
            {
                Dispatcher.BeginInvoke(new Action(() => { DeleteListLine(oldName); }));
                Dispatcher.BeginInvoke(new Action(() => { AddListLine(name); }));
            }
            else if (watcherChangeTypes == System.IO.WatcherChangeTypes.Deleted)
            {
                Dispatcher.BeginInvoke(new Action(() => { DeleteListLine(name); }));
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => { AddListLine(name); }));
            }
        }

        public void DeleteListLine(string text)
        {
            this.tvi.Items.Remove(text);
        }
 
        public void AddListLine(string text)
        {
            this.tvi.Items.Add(text);
        }
    }
}
