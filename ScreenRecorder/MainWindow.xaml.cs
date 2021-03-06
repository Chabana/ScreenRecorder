﻿/*#################################################################################################
*Project : ScreenRecorder
*Developped by : Daniel de Carvalho Fernandes, Michael Caraccio & Khaled Chabbou
*Date : 27 April 2015
*#################################################################################################*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Serialization;
using AForge.Video;
using AForge.Video.FFMPEG;
using Hardcodet.Wpf.TaskbarNotification;
using NHotkey;
using NHotkey.Wpf;
using Application = System.Windows.Application;
using Image = System.Drawing.Image;
using RadioButton = System.Windows.Controls.RadioButton;
using TextBox = System.Windows.Controls.TextBox;

namespace ScreenRecorder
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        // Writer for the video files
        private readonly VideoFileWriter _writer;
        // Know if a video is being recorded or not
        private bool _rec = true;

        // Count number of frames
        private UInt32 _frameCount;
        // Set frame rate to 25 fps
        private int _frameRate = 25;
        // Set bit rate
        private int _bitRate = 4000000;

        // Create a virtual screen with height and width
        private readonly int _width = SystemInformation.VirtualScreen.Width;
        private readonly int _height = SystemInformation.VirtualScreen.Height;

        // Create a screen Capture Stream
        private ScreenCaptureStream _streamVideo;

        // Stop the watcher
        readonly Stopwatch _stopwatch = new Stopwatch();

        // Tempo
        public int WaitLimit = 4000; //ms
        // List of filters (By name, by size, ...)
        private List<string> _listFilters;
        // Video extensions that the application can support
        private readonly string[] _extensionsVideos = { ".mp4", ".wmv" };
        // Photo extensions that the application can support
        private readonly string[] _extensionsPictures = { ".jpeg", ".png", ".bmp", ".gif", ".tiff" };
        // Set the default format of image to "png"
        private ImageFormat _imageFormat = ImageFormat.Png;
        private string _imageExtension = ".png";
        // Set the default format of video to "mp4"
        private VideoCodec _videoFormat = VideoCodec.MPEG4;
        private string _videoExtension = ".mp4";

        // Set the main folder where the videos and photos are saved and loaded
        private string _mainFolderPath = "c:\\ScreenRecorder";
        // Take the picture name
        private string _pictureName = "";
        private string _videoName = "";

        private Thread _thPos;
        private Thread _thdraw;
        private System.Drawing.Point _currentPoint;

        private string _fileSerialize = "serializeEmail.xml";

        private bool isSystem = false;
        private bool isTreeView = true;

        //##########################################################################################################################
        //######################################## Constructor -> Initialize the application ##############################


        /// <summary>
        /// Constructor to initialize the components
        /// </summary>
        public MainWindow()
        {

            InitializeComponent();

            //Minimize the window and don't put in the taskbar and set it to hidden
            ShowInTaskbar = false;
            WindowState = WindowState.Minimized;
            Visibility = Visibility.Hidden;

            // Set the shortcuts for the different functionalities
            HotkeyManager.Current.AddOrReplace("Screenshots", Key.S, ModifierKeys.Control | ModifierKeys.Alt, OnStartScreenshots);
            HotkeyManager.Current.AddOrReplace("VideoCapture", Key.V, ModifierKeys.Control | ModifierKeys.Alt, OnStartVideocapture);
            HotkeyManager.Current.AddOrReplace("ExitApplication", Key.Q, ModifierKeys.Control | ModifierKeys.Alt, OnExitApplication);
            HotkeyManager.Current.AddOrReplace("OpenApplication", Key.O, ModifierKeys.Control | ModifierKeys.Alt, OnOpenApplication);

            // Set the list of filters (by name, by date and by size)
            _listFilters = new List<string>
            {
                "Date descending",
                "Name descending",
                "Size descending",
                "Date ascending",
                "Name ascending",
                "Size ascending"
            };

            // Set properties to the filter
            FilterCombobox.IsEditable = true;
            FilterCombobox.IsTextSearchEnabled = true;
            FilterCombobox.ItemsSource = _listFilters;

            // Can drag the main window
            MouseDown += MainWindow_MouseDown;

            // Initialize a new folder with the mainFolderPath
            Folder folder = new Folder();
            folder.FullPath = _mainFolderPath;

            // Get the files of the mainFolderPath
            var observableCollection = folder.Files;

            if (observableCollection == null) try
                {
                    throw new ArgumentNullException(@"observableCollection");
                }
                catch (ArgumentNullException argumentNullException)
                {
                }

            // At the beginning of the application, add the files to the list
            foreach (var fileInfo in observableCollection)
            {
                AddListLine(fileInfo.Name);
            }

            // Set the properties to show the baloon
            const string title = "ScreenRecorder";
            const string text = "The application has been minimized";
            ShowStandardBalloon(title, text);

            // Initialize the video file writer
            _writer = new VideoFileWriter();

            // Set the button to send the e-mail to false
            BtnSendImageEmail.IsEnabled = false;
            BtnSendVideoEmail.IsEnabled = false;

            LblEmailSucced.Visibility = Visibility.Hidden;

            if (File.Exists(_fileSerialize))
            {
                TxtEmailSave.Text = DeserializeFromXml();
                BtnEmailSave.Content = "Edit";
                TxtEmailSave.IsEnabled = false;
            }
            else
            {
                using (FileStream fs = File.Create(_fileSerialize))
                {

                }
            }

            Update();
        }

        /// <summary>
        /// Count the videos in the main folder
        /// </summary>
        /// <returns>Return the number of videos in the main folder</returns>
        private int CountVideoInFolder()
        {
            return Directory.GetFiles(_mainFolderPath, "*.wmv", SearchOption.AllDirectories).Length + Directory.GetFiles(_mainFolderPath, "*.mp4", SearchOption.AllDirectories).Length;
        }

        /// <summary>
        /// Count the images in the main folder
        /// </summary>
        /// <returns>Return the number of images in the main folder</returns>
        private int CountImageInFolder()
        {
            int countbmp = Directory.GetFiles(_mainFolderPath, "*.bmp", SearchOption.AllDirectories).Length;
            int counttiff = Directory.GetFiles(_mainFolderPath, "*.tiff", SearchOption.AllDirectories).Length;
            int countjpeg = Directory.GetFiles(_mainFolderPath, "*.jpeg", SearchOption.AllDirectories).Length;
            int countjpg = Directory.GetFiles(_mainFolderPath, "*.jpg", SearchOption.AllDirectories).Length;
            int countgif = Directory.GetFiles(_mainFolderPath, "*.gif", SearchOption.AllDirectories).Length;
            int countpng = Directory.GetFiles(_mainFolderPath, "*.png", SearchOption.AllDirectories).Length;

            return countbmp + counttiff + countjpeg + countjpg + countgif + countpng;
        }
        //##########################################################################################################################
        //######################################## Exit application from menu or shortcut ##############################

        /// <summary>
        /// When pressed the shortcut to close the window, exit the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExitApplication(object sender, HotkeyEventArgs e)
        {
            ExitApplication();
        }

        /// <summary>
        /// Exit the current application
        /// </summary>
        private void ExitApplication()
        {
            //Quit the application
            Application.Current.Shutdown();
        }

        /// <summary>
        /// When clicking the menu button, quit the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuQuit_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        //##########################################################################################################################
        //######################################## Start video capture from menu or shortcut ##############################


        /// <summary>
        /// When pressed the shortcut to launch the video recording, launche the video recording
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartVideocapture(object sender, HotkeyEventArgs e)
        {
            if (_rec)
            {
                _rec = false;
            }
            
            StartVideocapture();
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        /// <summary>
        /// Start the video recording
        /// </summary>
        private void StartVideocapture()
        {
            // Test if the menu caputre equals to Start video capture
            if ((string)MenuCaptureVideo.Header == "Start Video Capture")
            {
                // Show the balloon and change the title of the menu button to stop video capture
                const string title = "ScreenRecorder";
                const string text = "Video has started recording";
                MenuCaptureVideo.Header = "Stop Video Capture";
                ShowStandardBalloon(title, text);

                // Set the different properties to start the recording
                if (_rec == false)
                {
                    Console.WriteLine(@"Recording has started");
                    _rec = true;

                    _frameCount = 0;

                    var time = DateTime.Now.ToString("dd-MM-yy HH.mm.ss");
                    var fullName = _mainFolderPath + "\\" + "video " + time;

                    /*if (_writer.IsOpen)
                    {
                        //_writer.Dispose();
                        _writer.Close();
                        //_stopwatch.Reset();
                        //_streamVideo.SignalToStop();
                    }*/
                    try
                    {

                        // Write the video with the name, frame rate, extension, ...
                        _writer.Open(fullName + _videoExtension,
                            _width,
                            _height,
                            _frameRate,
                            _videoFormat,
                            _bitRate);
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

                // Video has been recorded, stop the process and show the baloon
                const string title = "ScreenRecorder";
                const string text = "Video has been saved (C:\\ScreenRecorder)";
                ShowStandardBalloon(title, text);
                isTreeView = true;
                StopVideoRecording();
                MenuCaptureVideo.Header = "Start Video Capture";
                Console.WriteLine(@"Recording has stopped");
            }
        }

        /// <summary>
        /// Main process to launch the video recording
        /// </summary>
        private void Process()
        {
            try
            {
                var screenArea = Rectangle.Empty;
                foreach (var screen in Screen.AllScreens)
                {
                    screenArea = Rectangle.Union(screenArea, screen.Bounds);
                }

                _streamVideo = new ScreenCaptureStream(screenArea);

                _streamVideo.NewFrame += video_NewFrame;

                _streamVideo.Start();

                _stopwatch.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        /// <summary>
        /// Calls the event to start the video recording
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            {
                try
                {
                    if (_rec)
                    {
                        Bitmap bitmap = eventArgs.Frame;


                        _thPos = new Thread(
                        delegate()
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => this.GetPosition()));
                        });
                        _thPos.Start();

                        _thdraw = new Thread(
                        delegate()
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                                () =>
                                {

                                    SolidBrush myBrush = new SolidBrush(System.Drawing.Color.Red);

                                    Graphics g = Graphics.FromImage(bitmap);

                                    g.SmoothingMode = SmoothingMode.AntiAlias;
                                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                                    g.FillEllipse(myBrush, new Rectangle(_currentPoint.X, _currentPoint.Y, 25, 25));
                                    myBrush.Dispose();

                                    g.Flush();

                                    _writer.WriteVideoFrame(bitmap);
                                }));
                        }
                            );

                        _thdraw.Start();


                    }
                    else
                    {
                        _thdraw.Abort();
                        _thPos.Abort();
                        _stopwatch.Reset();
                        Thread.Sleep(300);
                        _streamVideo.SignalToStop();
                        Thread.Sleep(300);
                        _writer.Close();
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }

        /// <summary>
        /// Stop the video recording when pressed to the menu button or the shortcut
        /// </summary>
        private void StopVideoRecording()
        {
            // Set to true for next recording
            _rec = false;
            try
            {
                _rec = false;
                Console.WriteLine(@"FILE SAVED !!!!");


                FileContentCounter.Content = "You have : " + CountVideoInFolder() + (CountVideoInFolder() < 2 ? " video " : " videos ") + " and " + CountImageInFolder() +
                                         (CountImageInFolder() < 2 ? " image" : " images");

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        /// <summary>
        /// When the menu button clicked, start the video capture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuCaptureVideo_Click(object sender, RoutedEventArgs e)
        {
            StartVideocapture();
        }


        //##########################################################################################################################
        //######################################## Start screenshots from menu or shortcut ##############################

        /// <summary>
        /// Start taking a screenshot with the shortcut for that action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartScreenshots(object sender, HotkeyEventArgs e)
        {
            StartScreenshots();
        }

        /// <summary>
        /// Main process to start to take the screenshot
        /// </summary>
        private void StartScreenshots()
        {
            Rectangle screenArea = Rectangle.Empty;

            foreach (Screen screen in Screen.AllScreens)
            {
                screenArea = Rectangle.Union(screenArea, screen.Bounds);
            }

            ScreenCaptureStream stream = new ScreenCaptureStream(screenArea);

            stream.NewFrame += dev_NewFrame;

            stream.Start();

            Console.WriteLine(@"Screenshot taken");

            const string title = "ScreenRecorder";
            const string text = "Screenshot has been saved (C:\\ScreenRecorder)";
            isTreeView = true;
            ShowStandardBalloon(title, text);

            // File counter of images
            Thread.Sleep(100);
            FileContentCounter.Content = "You have : " + CountVideoInFolder() + (CountVideoInFolder() < 2 ? " video " : " videos ") + " and " + CountImageInFolder() +
                             (CountImageInFolder() < 2 ? " image" : " images");
        }

        /// <summary>
        /// Take screenshots by clicking the menu button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuScreenshot_Click(object sender, RoutedEventArgs e)
        {
            StartScreenshots();
        }

        /// <summary>
        /// Event to take a screenshot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void dev_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap bitmap = eventArgs.Frame.Clone() as Bitmap;
                ((ScreenCaptureStream)sender).SignalToStop();
                Image img = bitmap;

                var time = DateTime.Now.ToString("dd-MM-yy HH.mm.ss");
                var fullName = "image " + time + _imageExtension;


                fullName = Path.Combine(_mainFolderPath, fullName);
                img.Save(fullName, _imageFormat);


                //File counter for the images in the folder
                FileContentCounter.Content = "You have : " + CountVideoInFolder() + (CountVideoInFolder() < 2 ? " video " : " videos ") + " and " + CountImageInFolder() +
                                         (CountImageInFolder() < 2 ? " image" : " images");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //##########################################################################################################################
        //######################################## Start open application from menu or shortcut ##############################

        /// <summary>
        /// Shortcut to open the main application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenApplication(object sender, HotkeyEventArgs e)
        {
            OpenApplication();
        }

        /// <summary>
        /// Function to open the main application
        /// </summary>
        private void OpenApplication()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
                Visibility = Visibility.Visible;
                ShowInTaskbar = true;
                //hide balloon
                MyNotifyIcon.HideBalloonTip();
            }
        }

        /// <summary>
        /// Click the menu button to open the main window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenApplication();
        }

        //##########################################################################################################################
        //######################################## Can drag the main application ##############################

        /// <summary>
        /// Drag the main window
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

        //##########################################################################################################################
        //######################################## Can minimize the main application ##############################


        /// <summary>
        /// Click the button to minimize the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Minimized;
                ShowInTaskbar = false;
                const string title = "ScreenRecorder";
                const string text = "The application has been minimized";
                ShowStandardBalloon(title, text);

            }
        }

        //##########################################################################################################################
        //######################################## Show balloons to warn the user ##############################

        /// <summary>
        /// Function to show the different balloons of the application, to warn the user
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        private void ShowStandardBalloon(string title, string text)
        {
            //show balloon with built-in icon
            MyNotifyIcon.ShowBalloonTip(title, text, BalloonIcon.Error);

            //show balloon with custom icon
            MyNotifyIcon.ShowBalloonTip(title, text, MyNotifyIcon.Icon);
        }

        //##########################################################################################################################
        //######################################## Part for the files update (Create, Delete, Rename) ##############################

        /// <summary>
        /// Update the tree view
        /// </summary>
        private void Update()
        {
            isTreeView = false;

            if (string.IsNullOrWhiteSpace(_mainFolderPath))
                return;

            //Create a watcher for the different files
            FileSystemWatcher watcher = new FileSystemWatcher();

            watcher.Path = _mainFolderPath;

            // Filter the watcher
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.Size | NotifyFilters.DirectoryName | NotifyFilters.FileName;

            // All the extensions
            string[] extensions = { "*.jpeg", "*.mp4", "*.wmv", "*.png", "*.bmp", "*.gif", "*.tiff", "*.mpeg" };

            List<FileSystemWatcher> watchersExtension = new List<FileSystemWatcher>();

            foreach (String extension in extensions)
            {
                FileSystemWatcher w = new FileSystemWatcher { Filter = extension };
                w.Changed += fileSystemWatcher_Changed;
                watchersExtension.Add(w);
            }

            //Watcher for the CRUD
            //watcher.Created += fileSystemWatcher_Created;
            watcher.Changed += fileSystemWatcher_Changed;
            watcher.Deleted += fileSystemWatcher_Deleted;
            watcher.Renamed += fileSystemWatcher_Renamed;

            watcher.EnableRaisingEvents = true;

            FileContentCounter.Content = "You have : " + CountVideoInFolder() + (CountVideoInFolder() < 2 ? " video " : " videos ") + " and " + CountImageInFolder() +
                                         (CountImageInFolder() < 2 ? " image" : " images");
        }

        /// <summary>
        /// Watcher for the creation of a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            DisplayFileSystemWatcherInfo(e.ChangeType, e.Name);
        }

        /// <summary>
        /// Watcher for the changement of a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            DisplayFileSystemWatcherInfo(e.ChangeType, e.Name);
        }

        /// <summary>
        /// Watcher when a file is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            DisplayFileSystemWatcherInfo(e.ChangeType, e.Name);
        }

        /// <summary>
        /// Watcher when a file is renamed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            DisplayFileSystemWatcherInfo(e.ChangeType, e.Name, e.OldName);
        }

        /// <summary>
        /// Function to launch the different delegates
        /// </summary>
        /// <param name="watcherChangeTypes"></param>
        /// <param name="name"></param>
        /// <param name="oldName"></param>
        void DisplayFileSystemWatcherInfo(WatcherChangeTypes watcherChangeTypes, string name, string oldName = null)
        {
            if (watcherChangeTypes == WatcherChangeTypes.Renamed)
            {
                Dispatcher.BeginInvoke(new Action(() => { DeleteListLine(oldName); }));
                Dispatcher.BeginInvoke(new Action(() => { AddListLine(name); }));
            }
            else if (watcherChangeTypes == WatcherChangeTypes.Deleted)
            {
                Dispatcher.BeginInvoke(new Action(() => { DeleteListLine(name); }));
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => { AddListLine(name); }));
            }
        }

        /// <summary>
        /// Delete all the files from the list
        /// </summary>
        /// <param name="text"></param>
        public void DeleteListLine(string text)
        {
            if (!IsFileLocked(new FileInfo(_mainFolderPath + "\\" + text)))
            {
                Tvi.Items.Remove(text + " - " + GetFileSize(_mainFolderPath + "\\" + text).ToString("0.0") + " Mo");
            }
        }

        /// <summary>
        /// Add all the files to the list
        /// </summary>
        /// <param name="text"></param>
        public void AddListLine(string text)
        {
            try
            {
                if (!IsFileLocked(new FileInfo(_mainFolderPath + "\\" + text)))
                {
                    Tvi.Items.Add(text + " - " + GetFileSize(_mainFolderPath + "\\" + text).ToString("0.0") + " Mo");
                }
               
            }
            catch (SecurityException securityException)
            {
            }
        }

        //##########################################################################################################################
        //######################################## Part for the video player and image viewer ##############################

        private bool _mediaPlayerIsPlaying;
        private bool _userIsDraggingSlider;

        /// <summary>
        /// Tick for the slider to show the time for the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            if ((MePlayer.Source != null) && (MePlayer.NaturalDuration.HasTimeSpan) && (!_userIsDraggingSlider))
            {
                SliProgress.Minimum = 0;
                SliProgress.Maximum = MePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                SliProgress.Value = MePlayer.Position.TotalSeconds;
            }
        }

        /// <summary>
        /// Open the stream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
        }

        /// <summary>
        /// Play the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (MePlayer != null) && (MePlayer.Source != null);
        }

        /// <summary>
        /// Execute the playing of the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MePlayer.Play();
            _mediaPlayerIsPlaying = true;
        }

        /// <summary>
        /// Click pause to pause the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _mediaPlayerIsPlaying;
        }

        /// <summary>
        /// Pause the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MePlayer.Pause();
        }

        /// <summary>
        /// Click stop to stop the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _mediaPlayerIsPlaying;
        }

        /// <summary>
        /// Stop the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MePlayer.Stop();
            _mediaPlayerIsPlaying = false;
        }

        /// <summary>
        /// Can drag the slider of vieo time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            _userIsDraggingSlider = true;
        }

        /// <summary>
        /// Function to drag the slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _userIsDraggingSlider = false;
            MePlayer.Position = TimeSpan.FromSeconds(SliProgress.Value);
        }

        /// <summary>
        /// Event to the value changed of the slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LblProgressStatus.Text = TimeSpan.FromSeconds(SliProgress.Value).ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Select an item of the tree view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrvStructure_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Tvi.IsSelected) return;

            //Check for the videos and images
            foreach (string extension in _extensionsVideos)
            {
                if (e.NewValue.ToString().Contains(extension))
                {

                    // Récupération du filename (sans la taille du fichier)
                    string strfilename = (string)e.NewValue;
                    int foundS1 = strfilename.IndexOf(" ");
                    int foundS2 = strfilename.IndexOf(" ", foundS1 + 1);
                    int foundS3 = strfilename.IndexOf(" ", foundS2 + 1);

                    strfilename = strfilename.Remove(foundS3, (strfilename.Length) - foundS3);

                    Tabcontroler.SelectedItem = 0;
                    Tabcontroler.SelectedIndex = 0;
                    DispatcherTimer timerMedia = new DispatcherTimer();
                    MePlayer.Stretch = Stretch.Fill;
                    MePlayer.Source = new Uri(_mainFolderPath + "\\" + strfilename);
                    timerMedia.Interval = TimeSpan.FromSeconds(1);
                    timerMedia.Tick += timer_Tick;
                    timerMedia.Start();

                    var fileName = _mainFolderPath + "\\" + strfilename;
                    BtnSendVideoEmail.IsEnabled = true;

                    _videoName = fileName;

                    
                        VideoFileReader reader = new VideoFileReader();

                        // open video file
                        try
                        {
                            reader.Open(fileName);
                        }
                        catch (Exception exp)
                        {
                            Console.WriteLine(exp.Message);
                        }

                    try
                    {
                        DateTime fileCreatedDate = File.GetCreationTime(fileName);

                        VideoFilename.Text = Path.GetFileName(fileName);
                        VideoExtension.Text = reader.CodecName;
                        VideoCreated.Text = fileCreatedDate.ToString(CultureInfo.CurrentCulture);
                        VideoWidth.Text = reader.Width.ToString();
                        VideoHeight.Text = reader.Height.ToString();

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        ShowStandardBalloon("Bad File Format", "You have created, deleted or renamed this file in your file system !");
                    }
                        
                    
                    
                }
            }

            foreach (string extension in _extensionsPictures)
            {
                if (e.NewValue.ToString().Contains(extension))
                {
                    Tabcontroler.SelectedItem = 1;
                    Tabcontroler.SelectedIndex = 1;

                    // Récupération du filename (sans la taille du fichier)
                    string strfilename = (string)e.NewValue;
                    int foundS1 = strfilename.IndexOf(" ");
                    int foundS2 = strfilename.IndexOf(" ", foundS1 + 1);
                    int foundS3 = strfilename.IndexOf(" ", foundS2 + 1);

                    strfilename = strfilename.Remove(foundS3, (strfilename.Length) - foundS3);

                    var fileName = _mainFolderPath + "\\" + strfilename;
                    _pictureName = fileName;

                    BtnSendImageEmail.IsEnabled = true;


                    try
                    {
                        BitmapSource img = BitmapFrame.Create(new Uri(fileName, UriKind.RelativeOrAbsolute));

                        BitmapMetadata mdata = (BitmapMetadata) img.Metadata;

                        DateTime fileCreatedDate = File.GetCreationTime(fileName);

                        if (mdata != null)
                        {
                            ImageFilename.Text = Path.GetFileName(fileName);
                            ImageHeight.Text = img.PixelHeight.ToString();
                            ImageWidth.Text = img.PixelWidth.ToString();
                            ImageExtension.Text = mdata.Format;
                            ImageCreated.Text = fileCreatedDate.ToString(CultureInfo.CurrentCulture);
                        }

                        TabImage.Source = new BitmapImage(new Uri(fileName, UriKind.RelativeOrAbsolute));
                    }
                    catch (Exception ex)
                    {
                        ShowStandardBalloon("Bad File Format", "You have created, deleted or renamed this file in your file system !");

                    }


                }
            }
        }

        //##########################################################################################################################
        //######################################## Part for filters by name, by size or by date ascending and descending ##############################

        private float GetFileSize(string path)
        {
            return (float)(new FileInfo(path).Length / 1000000.0);
        }

        /// <summary>
        /// Event to change the selection of a filter in the combobox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void filterCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Folder folder = new Folder();
            var observableCollection = folder.Files;

            if (observableCollection == null) throw new ArgumentNullException(@"observableCollection");

            //At the beginning, delete all the medias
            foreach (var fileInfo in observableCollection)
            {
                DeleteListLine(fileInfo.Name);
            }

            string[] fns = Directory.GetFiles(_mainFolderPath);

            //The different filters
            var sortBySizeDescending = from fn in fns orderby new FileInfo(fn).Length descending select fn;
            var sortByDateDescending = from fn in fns orderby new FileInfo(fn).LastWriteTime descending select fn;
            var sortByNameDescending = from fn in fns orderby new FileInfo(fn).Name descending select fn;
            var sortBySizeAscending = from fn in fns orderby new FileInfo(fn).Length ascending select fn;
            var sortByDateAscending = from fn in fns orderby new FileInfo(fn).LastWriteTime ascending select fn;
            var sortByNameAscending = from fn in fns orderby new FileInfo(fn).Name ascending select fn;

            // Switch to select a filter
            switch (FilterCombobox.SelectedItem as string)
            {
                case "Size descending":

                    foreach (string n in sortBySizeDescending)
                    {
                        AddListLine(Path.GetFileName(n));
                    }
                    break;
                case "Name descending":

                    foreach (string n in sortByNameDescending)
                    {
                        AddListLine(Path.GetFileName(n));
                    }
                    break;
                case "Date descending":

                    foreach (string n in sortByDateDescending)
                    {
                        AddListLine(Path.GetFileName(n));
                    }
                    break;
                case "Size ascending":

                    foreach (string n in sortBySizeAscending)
                    {
                        AddListLine(Path.GetFileName(n));
                    }
                    break;
                case "Name ascending":

                    foreach (string n in sortByNameAscending)
                    {
                        AddListLine(Path.GetFileName(n));
                    }
                    break;
                case "Date ascending":

                    foreach (string n in sortByDateAscending)
                    {
                        AddListLine(Path.GetFileName(n));
                    }
                    break;
                default:
                    Console.WriteLine(@"Can't read the files with the filters !");
                    break;
            }
        }

        //###################################################################################################
        //######################################## Part for the properties tab ##############################

        /// <summary>
        /// Select a radio button (unique) to select the format of the screenshot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonImageFormat(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;
            if (button == null || button.Content == null)
                return;


            switch (button.Content.ToString())
            {
                case "Bmp":
                    _imageFormat = ImageFormat.Bmp;
                    _imageExtension = ".bmp";
                    break;
                case "Gif":
                    _imageFormat = ImageFormat.Gif;
                    _imageExtension = ".gif";
                    break;
                case "Jpeg":
                    _imageFormat = ImageFormat.Jpeg;
                    _imageExtension = ".jpeg";
                    break;
                case "Png":
                    _imageFormat = ImageFormat.Png;
                    _imageExtension = ".png";
                    break;
                case "Tiff":
                    _imageFormat = ImageFormat.Tiff;
                    _imageExtension = ".tiff";
                    break;
                default:
                    Console.WriteLine(@"Bad image extension !");
                    break;
            }
        }

        /// <summary>
        /// Select a radio button (unique) to select the frame rate of the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonFrameRate(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;
            if (button == null || button.Content == null)
                return;

            if (button.IsChecked != null && button.IsChecked.Value)
                Console.WriteLine(button.Content.ToString());

            switch (button.Content.ToString())
            {
                case "10":
                    _frameRate = 10;
                    break;
                case "15":
                    _frameRate = 15;
                    break;
                case "20":
                    _frameRate = 20;
                    break;
                case "25":
                    _frameRate = 25;
                    break;
                case "48":
                    _frameRate = 48;
                    break;
                case "60":
                    _frameRate = 60;
                    break;
                default:
                    Console.WriteLine(@"Bad frame rate !");
                    break;
            }
        }

        /// <summary>
        /// Select a radio button (unique) for the video format
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonVideoFormat(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;

            if (button == null || button.Content == null)
                return;
            if (button.IsChecked != null && button.IsChecked.Value)
                Console.WriteLine(button.Content.ToString());

            switch (button.Content.ToString())
            {
                case "MPEG4":
                    _videoFormat = VideoCodec.MPEG4;
                    _videoExtension = ".mp4";
                    break;

                case "WMV":
                    _videoFormat = VideoCodec.WMV2;
                    _videoExtension = ".wmv";
                    break;

                default:
                    Console.WriteLine(@"Bad video extension !");
                    break;
            }
        }

        //###################################################################################################
        //######################################## Part for sending email ##############################

        /// <summary>
        /// Click the button to send a picture by email to open the email form window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSendImageEmail_Click(object sender, RoutedEventArgs e)
        {
            EmailForm emailForm = new EmailForm(_pictureName, TxtEmailSave.Text);
            emailForm.Show();
        }

        /// <summary>
        /// Get current position of the mouse
        /// </summary>
        private void GetPosition()
        {
            _currentPoint = System.Windows.Forms.Control.MousePosition;
        }

        /// <summary>
        /// When we want to save the e-mail, it will serialize to xml file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEmailSave_Click(object sender, RoutedEventArgs e)
        {
            bool emailSender = Regex.IsMatch(TxtEmailSave.Text.Trim(), @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

            if (BtnEmailSave.Content.Equals("Edit"))
            {
                LblEmailSucced.Visibility = Visibility.Hidden;
                TxtEmailSave.IsEnabled = true;
                BtnEmailSave.Content = "Save";
            }
            else if (BtnEmailSave.Content.Equals("Save"))
            {
                if (TxtEmailSave.Text == "")
                {
                    LblEmailSucced.Content = "Your email is empty";
                    LblEmailSucced.Foreground = new SolidColorBrush(Colors.Red);
                    LblEmailSucced.Visibility = Visibility.Visible;
                }

                else if (!emailSender)
                {
                    LblEmailSucced.Content = "Your email is not a valid email : YourEmail@gmail.com";
                    LblEmailSucced.Foreground = new SolidColorBrush(Colors.Red);
                    LblEmailSucced.Visibility = Visibility.Visible;
                }
                else if (!TxtEmailSave.Text.Contains("@gmail.com"))
                {
                    LblEmailSucced.Content = "Only Gmail account are accepted";
                    LblEmailSucced.Foreground = new SolidColorBrush(Colors.Red);
                    LblEmailSucced.Visibility = Visibility.Visible;
                }
                else
                {
                    LblEmailSucced.Content = "Your E-mail has been saved";
                    LblEmailSucced.Foreground = new SolidColorBrush(Colors.Green);
                    LblEmailSucced.Visibility = Visibility.Visible;
                    BtnEmailSave.Content = "Edit";
                    TxtEmailSave.IsEnabled = false;
                    SerializeToXml();
                }
            }
        }

        /// <summary>
        /// Serialization of the email
        /// </summary>
        public void SerializeToXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(string));
            TextWriter textWriter = new StreamWriter(_fileSerialize);
            serializer.Serialize(textWriter, TxtEmailSave.Text);
            textWriter.Close();
        }

        /// <summary>
        /// Deserialize the email
        /// </summary>
        /// <returns></returns>
        public string DeserializeFromXml()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(string));
            TextReader textReader = new StreamReader(_fileSerialize);
            string email = null;
            try
            {
                email = (string)deserializer.Deserialize(textReader);
            }
            catch (Exception)
            {

            }


            textReader.Close();

            return email;
        }

        /// <summary>
        /// Enables the button when a text is entered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtEmailSave_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text.Length > 0)
            {
                BtnEmailSave.IsEnabled = true;
            }
            else
            {
                BtnEmailSave.IsEnabled = false;
            }

        }

        private void btnSendVideoEmail_Click(object sender, RoutedEventArgs e)
        {
            EmailForm emailForm = new EmailForm(_videoName, TxtEmailSave.Text);
            emailForm.Show();
        }

        private void RadioBitRate(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;
            if (button == null || button.Content == null)
                return;

            if (button.IsChecked != null && button.IsChecked.Value)
                Console.WriteLine(button.Content.ToString());

            switch (button.Content.ToString())
            {
                case "1 Mbps":
                    _bitRate = 1000000;
                    break;
                case "2 Mbps":
                    _bitRate = 2000000;
                    break;
                case "4 Mbps":
                    _bitRate = 4000000;
                    break;
                case "6 Mbps":
                    _bitRate = 6000000;
                    break;
                case "8 Mbps":
                    _bitRate = 8000000;
                    break;
                default:
                    Console.WriteLine(@"Bad bit rate !");
                    break;
            }
        }


    }
}