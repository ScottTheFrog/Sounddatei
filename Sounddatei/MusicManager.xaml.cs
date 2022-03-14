using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Un4seen.Bass;

namespace Sounddatei
{
    public partial class MusicManager : Grid
    {
        public MusicManager()
        {
            InitializeComponent();
            this.DataContext = this;
            TimeSlider.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownTimeSlider), true);
            TimeSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(MouseUpTimeSlider), true);
            ConstantUpdate();
            _mySync = new SYNCPROC(onStreamEnded);
        }

        public BitmapImage playIco = new BitmapImage(new Uri(Path.GetFullPath(@"..\..\..\Resources\play.ico")));
        public BitmapImage pauseIco = new BitmapImage(new Uri(Path.GetFullPath(@"..\..\..\Resources\pause.ico")));
        public BitmapImage soundIco = new BitmapImage(new Uri(Path.GetFullPath(@"..\..\..\Resources\sound.ico")));
        public BitmapImage nosoundIco = new BitmapImage(new Uri(Path.GetFullPath(@"..\..\..\Resources\nosound.ico")));


        public ObservableCollection<string> fileQueue = new ObservableCollection<string>();
        public int fileQueuePosition = 0;
        public List<int> fileQueueListPosition = new List<int>() { 0 } ;
        public int mainStream;
        public float currentVolume = 0.5f;

        public bool autoPlaying = true;
        public bool randomSong = false;

        float currentSongLenght = 1f;
        bool movingTimeSlider;

        private SYNCPROC _mySync;
        Random rnd = new Random();
        List<int> randomHolder = new List<int>();

        public bool AddFileToQueue(string pathToFile)
        {
            if (!fileQueue.Contains(pathToFile))
            {
                fileQueue.Add(pathToFile);
                return true;
            }
            else
            {
                return false;
            }
        }
        private void onStreamEnded(int handle, int channel, int data, IntPtr user)
        {
            this.Dispatcher.Invoke(() =>
            {
                resumeButtonImage.Source = playIco;
            });

            if ( fileQueue.Count > 1)
            {
                ifRandom(1);

                if (autoPlaying && fileQueuePosition <= (fileQueue.Count - 1))
                    playStream(fileQueueListPosition[fileQueuePosition]);

            }


        }
        public void playStream(int index, bool noInstant = false)
        {
            if (fileQueueListPosition.Count >= 1)
            {
                Bass.BASS_StreamFree(mainStream);
                if (!noInstant)
                {
                    fileQueueListPosition[fileQueueListPosition.Count - 1] = index;
                    fileQueuePosition = fileQueueListPosition.Count - 1;
                }
                this.Dispatcher.Invoke(() =>
                {
                    resumeButtonImage.Source = pauseIco;
                    songName.Text = Path.GetFileName(fileQueue[index]);
                });
                mainStream = Bass.BASS_StreamCreateFile(fileQueue[index], 0L, 0L, BASSFlag.BASS_DEFAULT);
                Bass.BASS_ChannelSetSync(mainStream, BASSSync.BASS_SYNC_END, 0, _mySync, IntPtr.Zero);
                Bass.BASS_ChannelPlay(mainStream, false);
                Bass.BASS_ChannelSetAttribute(mainStream, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_VOL, currentVolume);
                currentSongLenght = (float)Bass.BASS_ChannelGetLength(mainStream);
            }
        }

        public void MovingVolumeSlider(object sender, RoutedEventArgs e)
        {
            currentVolume = (float)VolumeSlider.Value;
            Bass.BASS_ChannelSetAttribute(mainStream, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_VOL, currentVolume);
            if (currentVolume == 0)
                VolumeSliderImage.Source = nosoundIco;
            else
                VolumeSliderImage.Source = soundIco;
        }
        public void MouseDownTimeSlider(object sender, RoutedEventArgs e)
        {
            movingTimeSlider = true;
        }
        public void MouseUpTimeSlider(object sender, RoutedEventArgs e)
        {
            movingTimeSlider = false;
        }


        public void onNextButtonPressed(object sender, RoutedEventArgs e)
        {
            if (fileQueuePosition <= (fileQueue.Count - 2))
            {
                ifRandom(1);
                playStream(fileQueueListPosition[fileQueuePosition], true);
            }
        }
        public void onPreviousButtonPressed(object sender, RoutedEventArgs e)
        {
            if (fileQueuePosition > 0 && fileQueue.Count > 1)
            {
                ifRandom(-1);

                playStream(fileQueueListPosition[fileQueuePosition], true);
            }
        }


        public void onResumeButtonPressed(object sender, RoutedEventArgs e)
        {
            BASSActive status = Bass.BASS_ChannelIsActive(mainStream);
            if (status != BASSActive.BASS_ACTIVE_PLAYING)
            {
                this.Dispatcher.Invoke(() =>
                {
                    resumeButtonImage.Source = pauseIco;
                });
                Bass.BASS_ChannelPlay(mainStream, false);
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    resumeButtonImage.Source = playIco;
                });
                Bass.BASS_ChannelPause(mainStream);
            }
        }
        public void onRandomButtonPressed(object sender, RoutedEventArgs e)
        {
            if (randomSong)
            {
                randomButton.BorderBrush = Brushes.Transparent;
                randomSong = false;
            }
            else
            {
                randomButton.BorderBrush = Brushes.Black;
                randomSong = true;
            }
        }
        void ifRandom(int value)
        {
            int randomValue = rnd.Next(fileQueue.Count - 1);
            if (fileQueue.Count >= 10 + fileQueue.Count/4)
            {
                if ((randomHolder.Count >= 10 + fileQueue.Count / 4))
                {
                    randomHolder.RemoveAt(0);
                }
                if (value > 0)
                {
                    if (randomSong)
                    {
                        while (randomHolder.Contains(randomValue))
                        {
                            randomValue = rnd.Next(fileQueue.Count - 1);
                            
                        }
                        fileQueueListPosition.Add(randomValue);
                    }
                    else
                        fileQueueListPosition.Add(fileQueueListPosition[fileQueuePosition]+1);
                }
            }
            else
            {
                if (value > 0)
                {
                    if (randomSong)
                        fileQueueListPosition.Add(randomValue);
                    else
                        fileQueueListPosition.Add(fileQueueListPosition[fileQueuePosition]+1);
                }
            }
            randomHolder.Add(fileQueueListPosition[fileQueuePosition]);
            fileQueuePosition += value;
        }
        public void ClearQueue()
        {
            fileQueue.Clear();
            randomHolder.Clear();
            fileQueueListPosition.Clear();
            fileQueueListPosition.Add(0);
            fileQueuePosition = 0;
        }

        public async void ConstantUpdate()
        {
            while (true)
            {
                if (!movingTimeSlider)
                {
                    float value = (float)Bass.BASS_ChannelGetPosition(mainStream) / currentSongLenght;
                    TimeSlider.Value = value;
                }
                else
                {
                    float seconds = (float)TimeSlider.Value * currentSongLenght;
                    Bass.BASS_ChannelSetPosition(mainStream, Bass.BASS_ChannelBytes2Seconds(mainStream, (long)seconds));
                }
                TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(mainStream, (long)Bass.BASS_ChannelGetPosition(mainStream)));
                TimeSpan timeTotal = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(mainStream, (long)Bass.BASS_ChannelGetLength(mainStream)));
                this.Dispatcher.Invoke(() =>
                {
                    currentTimeText.Text = time.ToString(@"hh\:mm\:ss") +" / " +  timeTotal.ToString(@"hh\:mm\:ss");
                });

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
        }
    }
}
