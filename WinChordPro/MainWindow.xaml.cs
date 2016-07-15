using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SysIO = System.IO;
using System.Xml;
using System.Windows.Markup;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;
using System.Windows.Forms;
using System.IO;
using System.Collections;


namespace WinChordPro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// Performance tips https://msdn.microsoft.com/en-us/library/ms173196(v=vs.110).aspx
    public partial class MainWindow : Window
    {
        static Log l = new Log();
        public static String SONG_DIR = @"E:\Songs\";
        private List<SongObj> folderSongs = new List<SongObj>();
        private static SongObj currentSong = new SongObj();
        public static string appName = "Win Chord Pro";//AppDomain.CurrentDomain.FriendlyName.ToString();
        Settings mySettings;
        private int firstTouchId = -1;
        private Point point1, point2 = new Point();

        public MainWindow()
        {
            InitializeComponent();
            l.initialize();
            mySettings = new Settings();
            loadDirectory(mySettings.lastSongFolder);
            //Need to test what happens if lastSongLoaded does not exist
            if (mySettings.lastSongLoaded != "" && mySettings.lastSongLoaded != null)
            {
                //currentSong = folderSongs.First(p => p.title == e.AddedItems[0].ToString());
                try
                {
                    currentSong = folderSongs.First(p => p.filePath == mySettings.lastSongLoaded);
                    loadSong(currentSong);
                }
                catch (Exception e)
                {
                    l.log("Exception attempting to get a matching song [" + e.InnerException + "]");
                }
            }
        }

        private void loadDirectory(String myDirectory)
        {
            l.log("******************************************************");
            l.log("Opening directory [" + myDirectory + "]");
            string[] folderFiles;
            if (Directory.Exists(myDirectory))
                folderFiles = Directory.GetFileSystemEntries(myDirectory, "*.*pro");
            else
                folderFiles = Directory.GetFileSystemEntries(SONG_DIR, "*.*pro");

            currentSong = new SongObj(folderFiles[0]);
            SongObj thisSong;

            listBoxSongs.Items.Clear();
            foreach (string fileFound in folderFiles)
            {
                thisSong = new SongObj(fileFound);
                folderSongs.Add(thisSong);
                listBoxSongs.Items.Add(thisSong.title);
            }
        }

        private void buttonOpenDir_Click(object sender, RoutedEventArgs e)
        {
            l.log("Enter buttonOpenDir_Click");
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.SelectedPath = SONG_DIR;
                DialogResult result = folderDialog.ShowDialog();

                loadDirectory(folderDialog.SelectedPath);

                mySettings.lastSongFolder = folderDialog.SelectedPath;
            }
            loadSong(currentSong);

            //Options for file open dialog
            //openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = SONG_DIR;
            //openFileDialog.FilterIndex = 2;
            //openFileDialog.RestoreDirectory = true;
        }


        private void listBoxSongs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            l.log("Enter listBoxSongs_SelectionChanged");

            if (e.AddedItems.Count < 1) 
                return;

            currentSong = folderSongs.First(p => p.title == e.AddedItems[0].ToString());
            loadSong(currentSong);
        }

        private void loadSong(SongObj songToLoad)
        {
            l.log("Enter loadSong");

            if (!File.Exists(songToLoad.filePath))
            {
                l.log("File does not exist [" + songToLoad.filePath + "]");
                return;
            }
            songToLoad.loadSong();

            string title = songToLoad.title;
            string fileName = songToLoad.fileName;
            string parentFolder = songToLoad.parentFolder;
            string key = songToLoad.key;
            string fullPathToSong = SysIO.Path.Combine(parentFolder, fileName);
            Paragraph p;
            List<Tuple<string,string>> linesFromFile = songToLoad.processedLines;
            this.Title = title + " - " + appName;
            labelKey.Content = "Key: " + key;

            FlowDocument flowDocLyrics = new FlowDocument();
            flowDocLyrics.FontFamily = new FontFamily(mySettings.font);
            flowDocLyrics.LineHeight = 1;
            flowDocLyrics.FontSize = mySettings.fontSize;
            //Paragraph p = new Paragraph(new Run(title));
            //p.FontSize = 20;
            //flowDocLyrics.Blocks.Add(p);

            //p = new Paragraph(new Run("Key: " + key));
            //p.FontSize = 12;
            //flowDocLyrics.Blocks.Add(p);

            //highlighting https://msdn.microsoft.com/en-us/library/ms607370(v=vs.110).aspx
            if (linesFromFile == null){
                l.log("Could not find linesFromFile in [" + fullPathToSong + "]");
                return;
            }

            foreach(Tuple<string,string> line in linesFromFile)
            {
                string tempLine = "";
                string chordLine = line.Item1;
                string lyricLine = line.Item2;
                //if (!(line.IndexOf('[') < 0) && !(line.IndexOf(']') < 0))
                if(chordLine != "")
                {
                    tempLine = chordLine;
                    //l.log("Compare orig [" + line + "]");
                    //l.log("To processed [" + tempLine + "]");

                    p = new Paragraph();
                    p.Inlines.Add(new TextBlock()
                    {
                        Text = tempLine,
                        TextWrapping = TextWrapping.NoWrap,
                        FontFamily = new FontFamily(mySettings.font),
                        FontSize = mySettings.fontSize
                    });
                    //p.Background = Brushes.Yellow;
                    p.FontWeight = FontWeights.Normal;
                    p.Foreground = Brushes.Blue;
                    //p.LineHeight = 1;
                   // p.Margin = new Thickness(0);
                    //p.Padding = new Thickness(0);
                    //p.KeepWithNext = true;
                    flowDocLyrics.Blocks.Add(p);
                }

                p = new Paragraph();
                // p = new Paragraph(new Run(lyricLine));
                p.Inlines.Add(new TextBlock()
                {
                    Text = lyricLine,
                    TextWrapping = TextWrapping.NoWrap,
                    FontFamily = new FontFamily(mySettings.font),
                    FontSize = mySettings.fontSize

                });
                //p.FontWeight = FontWeights.Normal;
                p.Foreground = Brushes.Black;
                //p.LineHeight = 1;
                //p.Margin = new Thickness(0);
                //p.Padding = new Thickness(0);
                //p.KeepWithNext = false;

                //p.FontSize = mySettings.fontSize;
                //p.FontStyle = FontStyles.Normal;
                //p.TextAlignment = TextAlignment.Left;
                flowDocLyrics.Blocks.Add(p);
            }
            fdViewer.Document = flowDocLyrics;
            mySettings.lastSongLoaded = currentSong.filePath;
            mySettings.saveSettings();
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            mySettings.saveSettings();
        }

        private void IncreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            mySettings.increaseFontSize();
            loadSong(currentSong);
        }

        private void DecreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            mySettings.decreaseFontSize();
            loadSong(currentSong);
        }

        private void fdViewer_TouchMove(object sender, TouchEventArgs e)
        {
            //https://msdn.microsoft.com/en-us/library/system.windows.input.touchdevice(v=vs.110).aspx
            FlowDocumentScrollViewer _fdViewer = (FlowDocumentScrollViewer)sender as FlowDocumentScrollViewer;

            if (_fdViewer != null)
            {
                TouchPoint tp = e.GetTouchPoint(_fdViewer);

                //if(e.TouchDevice.Id == 

                if (e.TouchDevice.Id == firstTouchId)
                {
                    point1.X = tp.Position.X;
                    point1.Y = tp.Position.Y;
                }
                else if (e.TouchDevice.Id != firstTouchId)
                {
                    point2.X = tp.Position.X;
                    point2.Y = tp.Position.Y;

                    if (point1.Y > point2.Y)
                    {
                        l.log("Scrolling Up");
                    }
                }


            }
        }

        private void fdViewer_TouchDown(object sender, TouchEventArgs e)
        {
            FlowDocumentScrollViewer _fdViewer = (FlowDocumentScrollViewer)sender as FlowDocumentScrollViewer;

            if (_fdViewer != null)
            {
                e.TouchDevice.Capture(_fdViewer);
                if (e.TouchDevice.Id == -1)
                    firstTouchId = e.TouchDevice.Id;
            }
        }

        private void fdViewer_TouchUp(object sender, TouchEventArgs e)
        {
            FlowDocumentScrollViewer _fdViewer = (FlowDocumentScrollViewer)sender as FlowDocumentScrollViewer;

            if (_fdViewer != null && e.TouchDevice.Captured == _fdViewer)
            {
                _fdViewer.ReleaseTouchCapture(e.TouchDevice);
            }
        }

        private void KeyUp_Click(object sender, RoutedEventArgs e)
        {
            currentSong.raiseKey();
            loadSong(currentSong);
        }

        private void KeyDown_Click(object sender, RoutedEventArgs e)
        {
            currentSong.lowerKey();
            loadSong(currentSong);
        }

    }
}

