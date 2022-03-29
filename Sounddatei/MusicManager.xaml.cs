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
using Un4seen.Bass.AddOn.Enc;
using System.Windows.Interop;

namespace Sounddatei
{
    public partial class MusicManager : Grid
    {
        public MusicManager()
        {
            InitializeComponent();
            this.DataContext = this;
            Seekbar.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(SeekbarMouseDown), true);
            this.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(gridMouseUp), true);
            this.AddHandler(MouseLeaveEvent, new MouseEventHandler(gridLostMouseCapture), true);
            this.AddHandler(MouseMoveEvent, new MouseEventHandler(gridMouseMove), true);
            ConstantUpdate();
            _mySync = new SYNCPROC(onStreamEnded);
        }

        public BitmapSource pauseIco = Imaging.CreateBitmapSourceFromHIcon(ResourcesProgram.pause.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        public BitmapSource playIco = Imaging.CreateBitmapSourceFromHIcon(ResourcesProgram.play.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        public BitmapSource soundIco = Imaging.CreateBitmapSourceFromHIcon(ResourcesProgram.sound.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        public BitmapSource nosoundIco = Imaging.CreateBitmapSourceFromHIcon(ResourcesProgram.nosound.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

        public BitmapSource shuffleIco = Imaging.CreateBitmapSourceFromHIcon(ResourcesProgram.random.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        public BitmapSource activatedshuffleIco = Imaging.CreateBitmapSourceFromHIcon(ResourcesProgram.activatedshuffle.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        public BitmapSource loopIco = Imaging.CreateBitmapSourceFromHIcon(ResourcesProgram.loop.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        public BitmapSource activatedloopIco = Imaging.CreateBitmapSourceFromHIcon(ResourcesProgram.activatedloop.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());


        public List<string> fileQueue = new List<string>();
        public int fileQueuePosition = 0;
        public List<int> fileQueueListPosition = new List<int>() { 0 } ;
        public int mainStream;
        TimeSpan timeTotal;
        public float currentVolume = 0.5f;
        public float rememberedVolume = 0.5f;
        public bool soundMuted = false;

        public bool autoPlaying = true;
        public bool autoLoop = false;
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
            if (fileQueue.Count >= 1)
            {
                if (autoLoop)
                {
                    playStream(fileQueueListPosition[fileQueuePosition]);
                    return;
                }
                ifRandom(1);

                if (autoPlaying && fileQueueListPosition[fileQueuePosition] <= (fileQueue.Count - 1) || randomSong)
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
                timeTotal = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(mainStream, Bass.BASS_ChannelGetLength(mainStream)));

                //int encoder = BassEnc.BASS_Encode_Start(mainStream, "lame -r -x -s 44100 -b 128 -", Un4seen.Bass.AddOn.Enc.BASSEncode.BASS_ENCODE_NOHEAD, null, IntPtr.Zero);
                //BassEnc.BASS_Encode_CastInit(encoder, "localhost:8000", "scott", BassEnc.BASS_ENCODE_TYPE_MP3, "Scott Stream", "localhost:8000", "genre", null, null, 128, true);

                currentSongLenght = (float)Bass.BASS_ChannelGetLength(mainStream);
            }
        }

        public void MovingVolumeSlider(object sender, RoutedEventArgs e)
        {   
            if ((float)VolumeSlider.Value != 0)
            {
                rememberedVolume = (float)VolumeSlider.Value;
                currentVolume = rememberedVolume;
                soundMuted = false;
                VolumeButtonImage.Source = soundIco;
            }
            else
            {
                soundMuted = true;
                currentVolume = 0f;
                VolumeButtonImage.Source = nosoundIco;
            }
            Bass.BASS_ChannelSetAttribute(mainStream, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_VOL, currentVolume);
        }
        private void onVolumeButtonPressed(object sender, RoutedEventArgs e)
        {
            if (soundMuted)
            {
                soundMuted = false;
                currentVolume = rememberedVolume;
                VolumeButtonImage.Source = soundIco;
            }
            else
            {
                soundMuted = true;
                currentVolume = 0f;
                VolumeButtonImage.Source = nosoundIco;
            }
        }

        public void onNextButtonPressed(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(fileQueueListPosition[fileQueuePosition]);
            if (fileQueueListPosition[fileQueuePosition] <= (fileQueue.Count - 2) || randomSong)
            {
                ifRandom(1);
                playStream(fileQueueListPosition[fileQueuePosition], true);
            }
        }
        public void onPreviousButtonPressed(object sender, RoutedEventArgs e)
        {
            if (fileQueueListPosition[fileQueuePosition] > 0 && fileQueue.Count > 1)
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
                randomButton.Source = shuffleIco;
                randomSong = false;
            }
            else
            {
                randomButton.Source = activatedshuffleIco;
                randomSong = true;
            }
        }
        public void onLoopButtonPressed(object sender, RoutedEventArgs e)
        {
            if (autoLoop)
            {
                loopButton.Source = loopIco;
                autoLoop = false;
            }
            else
            {
                loopButton.Source = activatedloopIco;
                autoLoop = true;
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
                randomHolder.Add(fileQueueListPosition[fileQueuePosition]);
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
            fileQueuePosition += value;
        }
        private void gridMouseMove(object sender, MouseEventArgs e)
        {
            if (!movingTimeSlider)
            {
                return;
            }
            double MousePosition = Mouse.GetPosition(Seekbar).X;
            this.Seekbar.Value = SetProgressBarValue(MousePosition);
        }
        private void SeekbarMouseDown(object sender, MouseButtonEventArgs e)
        {
            movingTimeSlider = true;
            Bass.BASS_ChannelSetAttribute(mainStream, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_VOL, 0);
        }
        private void gridMouseUp(object sender, MouseButtonEventArgs e)
        {
            movingTimeSlider = false;
            Bass.BASS_ChannelSetAttribute(mainStream, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_VOL, currentVolume);
        }
        private void gridLostMouseCapture(object sender, MouseEventArgs e )
        {
            movingTimeSlider = false;
            Bass.BASS_ChannelSetAttribute(mainStream, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_VOL, currentVolume);
        }

        private double SetProgressBarValue(double MousePosition)
        {
            double ratio = MousePosition / Seekbar.ActualWidth;
            double ProgressBarValue = ratio * Seekbar.Maximum;
            return ProgressBarValue;
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

                    double value = (double)Bass.BASS_ChannelGetPosition(mainStream) / currentSongLenght * 100;
                    this.Seekbar.Value = value;
                }
                else
                {
                    float seconds = (float)Seekbar.Value / 100 * currentSongLenght;
                    Bass.BASS_ChannelSetPosition(mainStream, Bass.BASS_ChannelBytes2Seconds(mainStream, (long)seconds));
                }

                TimeSpan time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(mainStream, Bass.BASS_ChannelGetPosition(mainStream)));
                this.Dispatcher.Invoke(() =>
                {
                    currentTimeText.Text = time.ToString(@"hh\:mm\:ss") +" / " +  timeTotal.ToString(@"hh\:mm\:ss");
                });

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
        }
    }
}
