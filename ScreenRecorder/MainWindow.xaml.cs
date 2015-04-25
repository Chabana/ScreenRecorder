using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AForge.Video;
using AForge.Video.FFMPEG;
using Hardcodet.Wpf.TaskbarNotification;
using NHotkey;
using NHotkey.Wpf;
using Application = System.Windows.Application;
using Image = System.Drawing.Image;
using RadioButton = System.Windows.Controls.RadioButton;

namespace ScreenRecorder
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly VideoFileWriter _writer;
        private bool _rec;

        private Rectangle _screenSize = Screen.PrimaryScreen.Bounds;

        private UInt32 _frameCount;
        private int _frameRate = 25;

        private readonly int _width = SystemInformation.VirtualScreen.Width;
        private readonly int _height = SystemInformation.VirtualScreen.Height;

        private ScreenCaptureStream _streamVideo;

        readonly Stopwatch _stopwatch = new Stopwatch();

        public int WaitLimit = 4000; //ms
        private List<string> _listFilters;
        private readonly string[] _extensionsVideos = { ".mp4", ".wmv"};
        private readonly string[] _extensionsPictures = { ".jpeg", ".png", ".bmp", ".gif", ".tiff" };
        private ImageFormat _imageFormat = ImageFormat.Png;
        private string _imageExtension = ".png";
        private VideoCodec _videoFormat = VideoCodec.MPEG4;
        private string _videoExtension = ".mp4";

        //##########################################################################################################################
        //######################################## Constructor -> Initialize the application ##############################


        public MainWindow()
        {
            InitializeComponent();

            HotkeyManager.Current.AddOrReplace("Screenshots", Key.S, ModifierKeys.Control | ModifierKeys.Alt, OnStartScreenshots);
            HotkeyManager.Current.AddOrReplace("VideoCapture", Key.V, ModifierKeys.Control | ModifierKeys.Alt, OnStartVideocapture);
            HotkeyManager.Current.AddOrReplace("ExitApplication", Key.E, ModifierKeys.Control | ModifierKeys.Alt, OnExitApplication);
            HotkeyManager.Current.AddOrReplace("OpenApplication", Key.O, ModifierKeys.Control | ModifierKeys.Alt, OnOpenApplication);

            _listFilters = new List<string>
            {
                "By date descending",
                "By name descending",
                "By size descending",
                "By date ascending",
                "By name ascending",
                "By size ascending"
            };

            FilterCombobox.IsEditable = true;
            FilterCombobox.IsTextSearchEnabled = true;
            FilterCombobox.ItemsSource = _listFilters;

            MouseDown += MainWindow_MouseDown;

            Folder folder = new Folder();
            var observableCollection = folder.Files;

            if (observableCollection == null) try
            {
                throw new ArgumentNullException(@"observableCollection");
            }
            catch (ArgumentNullException argumentNullException)
            {
            }

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

            _writer = new VideoFileWriter();

            Update();
        }

        

        //##########################################################################################################################
        //######################################## Exit application from menu or shortcut ##############################


        private void OnExitApplication(object sender, HotkeyEventArgs e)
        {
            ExitApplication();
        }

        private void ExitApplication()
        {
            //Save the video screen in a file
            StopVideoRecording();

            //Quit the application
            Application.Current.Shutdown();
        }

        private void menuQuit_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        //##########################################################################################################################
        //######################################## Start video capture from menu or shortcut ##############################


        private void OnStartVideocapture(object sender, HotkeyEventArgs e)
        {
            StartVideocapture();
        }

        private void StartVideocapture()
        {
            if ((string)MenuCaptureVideo.Header == "Start Video Capture")
            {
                const string title = "Video has started recording";
                const string text = "Don't forget to stop the video recording";
                MenuCaptureVideo.Header = "Stop Video Capture";
                ShowStandardBalloon(title, text);


                if (_rec == false)
                {
                    Console.WriteLine(@"Recording has started");
                    _rec = true;

                    _frameCount = 0;

                    var time = DateTime.Now.ToString("d_MMM_yyyy_HH_mm_ssff");
                    var compName = Environment.UserName;
                    var fullName = "c:\\ScreenRecorder" + "\\" + compName.ToUpper() + "_" + time;

                    try
                    {
                        //Change FPS and the codec for video
                        _writer.Open(fullName + _videoExtension,
                            _width,
                            _height,
                            _frameRate,
                            _videoFormat);
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
                const string title = "Video has stopped recording";
                const string text = "You are not being recorded";
                ShowStandardBalloon(title, text);
                StopVideoRecording();
                MenuCaptureVideo.Header = "Start Video Capture";
                Console.WriteLine(@"Recording has stopped");
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

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (_rec)
                {
                    _writer.WriteVideoFrame(eventArgs.Frame);
                }

                else
                {
                    _stopwatch.Reset();
                    Thread.Sleep(500);
                    _streamVideo.SignalToStop();
                    Thread.Sleep(500);
                    _writer.Close();
                }
            }
            catch (Exception exception)
            {

                Console.WriteLine(exception.Message);
            }
        }

        private void StopVideoRecording()
        {
            _rec = false;
            try
            {
                _rec = false;


                Console.WriteLine(@"FILE SAVED !!!!");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private void menuCaptureVideo_Click(object sender, RoutedEventArgs e)
        {
            StartVideocapture();
        }


        //##########################################################################################################################
        //######################################## Start screenshots from menu or shortcut ##############################


        private void OnStartScreenshots(object sender, HotkeyEventArgs e)
        {
            StartScreenshots();
        }

        private void StartScreenshots()
        {
            Rectangle screenArea = Rectangle.Empty;

            foreach (Screen screen in Screen.AllScreens)
            {
                screenArea = Rectangle.Union(screenArea, screen.Bounds);
            }

            ScreenCaptureStream stream = new ScreenCaptureStream(screenArea);

            Thread.Sleep(1800);

            stream.NewFrame += dev_NewFrame;

            stream.Start();
            filterCombobox_SelectionChanged(FilterCombobox, null);

            Console.WriteLine("Screenshot taken");
        }

        /// <summary>
        /// Fonction pour prendre des screenshots
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuScreenshot_Click(object sender, RoutedEventArgs e)
        {

            StartScreenshots();

        }

        private void dev_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap bitmap = eventArgs.Frame.Clone() as Bitmap;
                ((ScreenCaptureStream)sender).SignalToStop();
                Image img = bitmap;
                string dirName = "c:\\ScreenRecorder";
                string flName = "Photo" + DateTime.Now.ToString("yyyyMMddhhmmss") + _imageExtension;
                flName = Path.Combine(dirName, flName);
                img.Save(flName, _imageFormat);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


        }


        //##########################################################################################################################
        //######################################## Start open application from menu or shortcut ##############################

        private void OnOpenApplication(object sender, HotkeyEventArgs e)
        {
            OpenApplication();
        }

        private void OpenApplication()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
                ShowInTaskbar = true;
                //hide balloon
                MyNotifyIcon.HideBalloonTip();
            }
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenApplication();
        }

        //##########################################################################################################################
        //######################################## Can drag the main application ##############################

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        //##########################################################################################################################
        //######################################## Can minimize the main application ##############################


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

        //##########################################################################################################################
        //######################################## Show balloons to warn the user ##############################
        
        private void ShowStandardBalloon(string title, string text)
        {
            //show balloon with built-in icon
            MyNotifyIcon.ShowBalloonTip(title, text, BalloonIcon.Error);

            //show balloon with custom icon
            MyNotifyIcon.ShowBalloonTip(title, text, MyNotifyIcon.Icon);
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

            string[] extensions = { "*.jpg", "*.mp4", "*.wmv", "*.png", "*.avi", "*.bmp" };

            List<FileSystemWatcher> watchersExtension = new List<FileSystemWatcher>();

            foreach (String extension in extensions)
            {
                FileSystemWatcher w = new FileSystemWatcher {Filter = extension};
                w.Changed += fileSystemWatcher_Changed;
                watchersExtension.Add(w);
            }

            watcher.Created += fileSystemWatcher_Created;
            
            watcher.Deleted += fileSystemWatcher_Deleted;
            watcher.Renamed += fileSystemWatcher_Renamed;

            watcher.EnableRaisingEvents = true;
        }

        void fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            
            DisplayFileSystemWatcherInfo(e.ChangeType, e.Name);
        }

        void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            DisplayFileSystemWatcherInfo(e.ChangeType, e.Name);
        }

        void fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
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
            else if (watcherChangeTypes == WatcherChangeTypes.Deleted)
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
            Tvi.Items.Remove(text);
        }
 
        public void AddListLine(string text)
        {
            Tvi.Items.Add(text);
        }

        //##########################################################################################################################
        //######################################## Part for the video player and image viewer ##############################

        private bool _mediaPlayerIsPlaying;
        private bool _userIsDraggingSlider;

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((MePlayer.Source != null) && (MePlayer.NaturalDuration.HasTimeSpan) && (!_userIsDraggingSlider))
            {
                SliProgress.Minimum = 0;
                SliProgress.Maximum = MePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                SliProgress.Value = MePlayer.Position.TotalSeconds;
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (MePlayer != null) && (MePlayer.Source != null);
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MePlayer.Play();
            _mediaPlayerIsPlaying = true;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _mediaPlayerIsPlaying;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MePlayer.Pause();
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _mediaPlayerIsPlaying;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MePlayer.Stop();
            _mediaPlayerIsPlaying = false;
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            _userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _userIsDraggingSlider = false;
            MePlayer.Position = TimeSpan.FromSeconds(SliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LblProgressStatus.Text = TimeSpan.FromSeconds(SliProgress.Value).ToString(@"hh\:mm\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        

        private void TrvStructure_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Tvi.IsSelected) return;
            foreach (string extension in _extensionsVideos)
            {
                if (e.NewValue.ToString().Contains(extension))
                {
                    Tabcontroler.SelectedItem = 0;
                    Tabcontroler.SelectedIndex = 0;
                    DispatcherTimer timerMedia = new DispatcherTimer();
                    MePlayer.Stretch = Stretch.Fill;
                    MePlayer.Source = new Uri("c:\\ScreenRecorder\\" + e.NewValue);
                    timerMedia.Interval = TimeSpan.FromSeconds(1);
                    timerMedia.Tick += timer_Tick;
                    timerMedia.Start();
                }
            }

            foreach (string extension in _extensionsPictures)
            {
                if (e.NewValue.ToString().Contains(extension))
                {
                    Tabcontroler.SelectedItem = 1;
                    Tabcontroler.SelectedIndex = 1;

                    var fileName = "c:\\ScreenRecorder\\" + e.NewValue;

                    BitmapSource img = BitmapFrame.Create(new Uri(fileName, UriKind.RelativeOrAbsolute));

                    BitmapMetadata mdata = (BitmapMetadata)img.Metadata;
                    
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
            }
        }

        //##########################################################################################################################
        //######################################## Part for filters by name, by size or by date ascending and descending ##############################


        private void filterCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Folder folder = new Folder();
            var observableCollection = folder.Files;

            if (observableCollection == null) throw new ArgumentNullException(@"observableCollection");
            Console.WriteLine(observableCollection.Count);
            foreach (var fileInfo in observableCollection)
            {
                DeleteListLine(fileInfo.Name);
            }
            const string dir = "c:\\ScreenRecorder";
            string[] fns = Directory.GetFiles(dir);

            var sortBySizeDescending = from fn in fns orderby new FileInfo(fn).Length descending select fn;
            var sortByDateDescending = from fn in fns orderby new FileInfo(fn).LastWriteTime descending select fn;
            var sortByNameDescending = from fn in fns orderby new FileInfo(fn).Name descending select fn;
            var sortBySizeAscending = from fn in fns orderby new FileInfo(fn).Length ascending select fn;
            var sortByDateAscending = from fn in fns orderby new FileInfo(fn).LastWriteTime ascending select fn;
            var sortByNameAscending = from fn in fns orderby new FileInfo(fn).Name ascending select fn;


            switch (FilterCombobox.SelectedItem as string)
            {
                case "By size descending":

                    foreach (string n in sortBySizeDescending)
                    {
                        Console.WriteLine(Path.GetFileName(n));
                        AddListLine(Path.GetFileName(n));
                    }
                    break;
                case "By name descending":

                    foreach (string n in sortByNameDescending)
                    {
                        Console.WriteLine(Path.GetFileName(n));
                        AddListLine(Path.GetFileName(n));
                    }
                    break;
                case "By date descending":

                    foreach (string n in sortByDateDescending)
                    {
                        Console.WriteLine(Path.GetFileName(n));
                        AddListLine(Path.GetFileName(n));
                    }
                    break;
                case "By size ascending":

                    foreach (string n in sortBySizeAscending)
                    {
                        Console.WriteLine(Path.GetFileName(n));
                        AddListLine(Path.GetFileName(n));
                    }
                    break;
                case "By name ascending":

                    foreach (string n in sortByNameAscending)
                    {
                        Console.WriteLine(Path.GetFileName(n));
                        AddListLine(Path.GetFileName(n));
                    }
                    break;
                case "By date ascending":

                    foreach (string n in sortByDateAscending)
                    {
                        Console.WriteLine(Path.GetFileName(n));
                        AddListLine(Path.GetFileName(n));
                    }
                    break;
            }
        }

        //###################################################################################################
        //######################################## Part for the properties tab ##############################

        private void RadioButtonImageFormat(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;
            if (button == null || button.Content == null)
                return;

            if (button.IsChecked != null && button.IsChecked.Value)
                Console.WriteLine(button.Content.ToString());

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
                    _imageFormat = ImageFormat.Png;
                    _imageExtension = ".tiff";
                    break;
                default:
                    Console.WriteLine("Unexpected error my friend !");
                    break;
            }
        }

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
                case "24":
                    _frameRate = 24;
                    break;
                case "48":
                    _frameRate = 48;
                    break;
                case "60":
                    _frameRate = 60;
                    break;
                default:
                    Console.WriteLine("Unexpected error my friend !");
                    break;
            }
        }

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
                    Console.WriteLine("Unexpected error my friend !");
                    break;
            }
        }
    }
}