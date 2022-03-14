using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Input;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Un4seen.Bass;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

namespace Sounddatei
{
    public partial class MainWindow : Window
    {
        static string[] imageExtensions = { ".APG", ".AVIF", ".GIF", ".JPEG", ".JPG" , ".PNG", ".SVG", ".WEBP" };
        static string[] videoExtensions = { ".WEBM", ".MKV", ".VOB", ".OGV", ".GIFV", ".AVI", ".MOV", ".QT", ".MP4", ".M4P", ".M4V", ".MPG", ".MPEG", ".M2V", ".3GP", ".3G2", ".FLV"};
        static string[] audioExtensions = { ".AAC", ".AAX", ".ACT", ".AA", ".AIFF", ".ALAC", ".AMR", ".APE", ".AU", ".AWB", ".M4A", ".M4B", ".M4P", ".MP3", ".MP2", ".MP1 ", ".MPC", ".OGG", ".OGA", ".MOGG", ".OPUS", ".WAV", ".WMA", ".WEBM", ".FLAC" };
        public List<string> pathList = new List<string> { "null"};
        public MainWindow()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
 
            this.DataContext = this;
            InitializeComponent();
            MusicList.AddHandler(MouseDoubleClickEvent, new MouseButtonEventHandler(OnMusicListMouseLeftButtonDown), true);
            PopulateTreeView(dialog.SelectedPath);
            ConstantUpdate();

        }
        async void ConstantUpdate()
        {
            while (true)
            {
                if (mainMusicManager.fileQueueListPosition.Count > 0)
                {
                listNumber.Text = mainMusicManager.fileQueueListPosition[mainMusicManager.fileQueuePosition] + 1 + "/" + (string)transferFileQueue.Count.ToString();
                }
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }

        public ObservableCollection<ListItem> transferFileQueue { get; set; } = new ObservableCollection<ListItem>();
        public void AddItem(int itemindex)
        {
            ListItem listfile = new ListItem(Path.GetFileName(mainMusicManager.fileQueue[itemindex]), mainMusicManager.fileQueue[itemindex]);
            var currentfilelengthstream = Bass.BASS_StreamCreateFile(mainMusicManager.fileQueue[itemindex], 0L, 0L, BASSFlag.BASS_DEFAULT);
            TimeSpan timeTotal = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(currentfilelengthstream, (long)Bass.BASS_ChannelGetLength(currentfilelengthstream)));
            listfile.Length = timeTotal.ToString(@"hh\:mm\:ss");
            transferFileQueue.Add(listfile);

        }
        public void OnClearButton(object sender, EventArgs e)
        {
            transferFileQueue.Clear();
            mainMusicManager.ClearQueue();
            listNumber.Text = "Cleared Queue...";
        }

        public void OnMusicListMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (MusicList.SelectedItems.Count != 0)
            {
                mainMusicManager.playStream(MusicList.SelectedIndex);
            }
        }

        public void OnMouseLeftButtonDown(object sender, EventArgs e)
        {
            if (!((TreeViewItem)sender).IsSelected)
            {
                return;
            }
            ItemCustom obj = sender as ItemCustom;

            string currentObjPath = Path.GetFullPath(obj.Path);
            if (obj.isDirectory)
            {
                if (!pathList.Contains(currentObjPath))
                {
                    PopulateTreeViewItem(currentObjPath, obj.list);
                }
            }
            else
            {
                if (IsAcceptableFileExtension(currentObjPath))
                {
                    if (mainMusicManager.AddFileToQueue(currentObjPath))
                    {
                        AddItem(mainMusicManager.fileQueue.Count - 1);
                    }
                    mainMusicManager.playStream(mainMusicManager.fileQueue.IndexOf(currentObjPath));
                }
            }
        }
        public void OnMouseRightButtonDown(object sender, MouseEventArgs e)
        {
            ItemCustom obj = sender as ItemCustom;
            string currentObjPath = Path.GetFullPath(obj.Path);
            
            if (obj.isDirectory)
            {
                DirectoryRecursive(currentObjPath);
                e.Handled = true;
            }
            else
            {
                if (IsAcceptableFileExtension(currentObjPath))
                {
                    if (mainMusicManager.AddFileToQueue(currentObjPath))
                    {
                        AddItem(mainMusicManager.fileQueue.Count - 1);
                        e.Handled = true;
                    }
                }
            }
        }
        public void DirectoryRecursive(string recursivePath)
        {
            string[] directoryEntries = Directory.GetDirectories(recursivePath);
            foreach (string directory in directoryEntries)
            {
                DirectoryRecursive(directory);
            }
            string[] fileEntries = Directory.GetFiles(recursivePath);
            foreach (string file in fileEntries)
            {
                if (IsAcceptableFileExtension(file))
                {
                    if (mainMusicManager.AddFileToQueue(file))
                    {
                        AddItem(mainMusicManager.fileQueue.Count - 1);
                    }
                }
            }
         }

        public void PopulateTreeViewItem(string path, ObservableCollection<ItemCustom> holder)
        {
            pathList.Add(path);
            Style doubleclickstyle = this.FindResource("doubleclick") as Style;
            string[] directoryEntries = Directory.GetDirectories(path);
            foreach (string directory in directoryEntries)
            {
                ItemCustom item = new ItemCustom();
                BitmapImage directoryImageBitmap = new BitmapImage(new Uri(Path.GetFullPath(@"..\..\..\Resources\folder.ico")));
                item.ImageBitMap = directoryImageBitmap;
                item.Path = directory;
                item.FileName = Path.GetFileName(directory);
                item.isDirectory = true;
                item.Style = doubleclickstyle;
                holder.Add(item);

            }
            string[] fileEntries = Directory.GetFiles(path);
            foreach (string file in fileEntries)
            {
                ItemCustom item = new ItemCustom();
                item.ImageBitMap = GetImageForFile(file);
                item.Path = file;
                item.FileName = Path.GetFileName(file);
                item.isDirectory = false;
                item.Style = doubleclickstyle;
                holder.Add(item);

            }
        }
        public void PopulateTreeView(string path)
        {
            pathList.Add(path);
            Style doubleclickstyle = this.FindResource("doubleclick") as Style;
            string[] directoryEntries = Directory.GetDirectories(path);
            foreach (string directory in directoryEntries)
            {
                ItemCustom item = new ItemCustom();
                BitmapImage directoryImageBitmap = new BitmapImage(new Uri(Path.GetFullPath(@"..\..\..\Resources\folder.ico")));
                item.ImageBitMap = directoryImageBitmap;
                item.Path = directory;
                item.FileName = Path.GetFileName(directory);
                item.isDirectory = true;
                item.Style = doubleclickstyle;
                myTreeView.Items.Add(item);

            }
            string[] fileEntries = Directory.GetFiles(path);
            foreach (string file in fileEntries)
            {
                ItemCustom item = new ItemCustom();
                item.ImageBitMap = GetImageForFile(file);
                item.Path = file;
                item.FileName = Path.GetFileName(file);
                item.isDirectory = false;
                item.Style = doubleclickstyle;
                myTreeView.Items.Add(item);

            }
        }
        BitmapImage GetImageForFile(string path)
        {
            if (Array.IndexOf(imageExtensions, Path.GetExtension(path).ToUpperInvariant()) != -1)
            {
                return new BitmapImage(new Uri(Path.GetFullPath(@"..\..\..\Resources\imagefile.ico")));
            }
            if (Array.IndexOf(audioExtensions, Path.GetExtension(path).ToUpperInvariant()) != -1)
            {
                return new BitmapImage(new Uri(Path.GetFullPath(@"..\..\..\Resources\file.ico")));
            }
            if (Array.IndexOf(videoExtensions, Path.GetExtension(path).ToUpperInvariant()) != -1)
            {
                return new BitmapImage(new Uri(Path.GetFullPath(@"..\..\..\Resources\videofile.ico")));
            }
            return new BitmapImage(new Uri(Path.GetFullPath(@"..\..\..\Resources\file.ico")));

        }
        bool IsAcceptableFileExtension(string path)
        {
            if (Array.IndexOf(videoExtensions, 
                Path.GetExtension(Path.GetFullPath(path)).ToUpperInvariant()) != -1 ||
                Array.IndexOf(audioExtensions, 
                Path.GetExtension(Path.GetFullPath(path)).ToUpperInvariant()) != -1)
            {
                return true;
            }
            return false;
        }
        public void AboutPopup(object sender, RoutedEventArgs e)
        {
            aboutPopup.IsOpen = true;
        }
        public void ExitApp(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

    }
}
