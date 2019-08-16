using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Un4seen.Bass;
using Un4seen.BassWasapi;
using System.IO;

namespace KriVisualizer
{
    public partial class MainWindow : Window
    {
        bool Draging = false;
        Point StartDragPoint = new Point();
        public List<VisualizerControl> VisualizerStack = new List<VisualizerControl>();
        bool RunVisualizer = false;
        public bool VisualizersRunning = false;
        private WASAPIPROC BassProcess;
        System.Windows.Forms.SaveFileDialog SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
        System.Windows.Forms.OpenFileDialog LoadFileDialog = new System.Windows.Forms.OpenFileDialog();
        private bool ProperClosed = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Opacity = 0;
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Saves"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Saves");

            SaveFileDialog.DefaultExt = ".txt";
            LoadFileDialog.DefaultExt = ".txt";
            SaveFileDialog.AddExtension = true;
            LoadFileDialog.AddExtension = true;

            AudioSourceCombobox.Items.Clear();
            int DeviceCount = BassWasapi.BASS_WASAPI_GetDeviceCount();
            for (int i = 0; i < DeviceCount; i++)
            {
                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                if (device.IsEnabled && device.IsLoopback)
                {
                    AudioSourceCombobox.Items.Add(string.Format("{0} - {1}", i, device.name));
                }
            }
            AudioSourceCombobox.SelectedIndex = 0;

            RefreshRateCombobox.Items.Clear();
            for (int i = 0; i < 105; i += 5)
                RefreshRateCombobox.Items.Add(i.ToString());
            RefreshRateCombobox.SelectedIndex = 0;

            if (File.Exists("Saves\\autosave.txt"))
                LoadConfig("Saves\\autosave.txt");

            await GI.FadeIn(this);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Draging = true;
            Mouse.Capture(SettingsWindowGrid);
            StartDragPoint = e.GetPosition(SettingsWindowGrid);
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Draging = false;
            Mouse.Capture(null);
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (Draging)
            {
                this.Left = (System.Windows.Forms.Cursor.Position.X - StartDragPoint.X);
                this.Top = (System.Windows.Forms.Cursor.Position.Y - StartDragPoint.Y);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            //Application.Current.Shutdown();
        }

        private void AddNewVisualizerButton_Click(object sender, RoutedEventArgs e)
        {
            VisualizerControl NewControl = new VisualizerControl(this);
            VisualizerStack.Add(NewControl);
            NewControl.Show();
            HideOrShowItemsCheckBox.IsChecked = true;
            RefreshVisualizerCountLabel();
        }

        private void HideOrShowItemsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < VisualizerStack.Count; i++)
                VisualizerStack[i].ShowInterface();
        }

        private void HideOrShowItemsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < VisualizerStack.Count; i++)
                VisualizerStack[i].HideInterface();
        }

        private async void StartVisalizersButton_Click(object sender, RoutedEventArgs e)
        {
            await StartVisualizer(true, AudioSourceCombobox.Items[AudioSourceCombobox.SelectedIndex].ToString());
        }

        private async void StopVisalizersButton_Click(object sender, RoutedEventArgs e)
        {
            await StartVisualizer(false, "");
        }

        async Task StartVisualizer(bool Start, string DeviceName)
        {
            if (Start)
            {
                if (RunVisualizer)
                {
                    RunVisualizer = false;
                    while (VisualizersRunning)
                        await Task.Delay(10);
                }
                if (BassWasapi.BASS_WASAPI_IsStarted())
                    BassWasapi.BASS_WASAPI_Stop(true);

                BassWasapi.BASS_WASAPI_Free();
                Bass.BASS_Free();

                BassProcess = new WASAPIPROC(Process);

                var array = (AudioSourceCombobox.Items[AudioSourceCombobox.SelectedIndex] as string).Split(' ');
                int devindex = Convert.ToInt32(array[0]);
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
                Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                bool result = BassWasapi.BASS_WASAPI_Init(devindex, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, BassProcess, IntPtr.Zero);
                if (!result)
                {
                    var error = Bass.BASS_ErrorGetCode();
                    MessageBox.Show(error.ToString());
                }

                BassWasapi.BASS_WASAPI_Start();

                RunVisualizer = true;
                VisualizersRunning = true;

                Task VisualizerThreadStart = new Task(delegate {
                    VisualizerThread();
                });
                VisualizerThreadStart.Start();
            }
            else
            {
                if (RunVisualizer)
                {
                    RunVisualizer = false;
                    while (VisualizersRunning)
                        await Task.Delay(10);
                }
                for (int i = 0; i < VisualizerStack.Count; i++)
                    VisualizerStack[i].VisualizerControlBeatZoneCanvas.Children.Clear();

                if (BassWasapi.BASS_WASAPI_IsStarted())
                    BassWasapi.BASS_WASAPI_Stop(true);

                BassWasapi.BASS_WASAPI_Free();
                Bass.BASS_Free();
            }
        }

        private int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }

        private void VisualizerThread()
        {
            int RefreshRate = 0;
            string DeviceName = "";
            List<VisualizerControl> _VisualizerStack = new List<VisualizerControl>();

            Dispatcher.Invoke(() =>
            {
                RefreshRate = Int32.Parse(RefreshRateCombobox.SelectedItem.ToString());
                DeviceName = AudioSourceCombobox.SelectedItem.ToString();
                _VisualizerStack = VisualizerStack;
            });

            DateTime VisualizerRPSCounter = new DateTime();
            System.Diagnostics.Stopwatch CalibrateRefreshRate = new System.Diagnostics.Stopwatch();
            float[] AudioData = new float[16384];
            List<List<double>> AudioDataPointStore = new List<List<double>>();
            int VisualizerUpdatesCounter = 0;
            VisualizersRunning = true;
            for (int i = 0; i < 256; i++)
                AudioDataPointStore.Add(new List<double>(new double[100]));

            while (RunVisualizer)
            {
                CalibrateRefreshRate.Restart();

                int ReturnValue = BassWasapi.BASS_WASAPI_GetData(AudioData, (int)BASSData.BASS_DATA_FFT16384);
                if (ReturnValue < -1) return;

                int X;
                double Y;
                int B0 = 0;

                for (X = 0; X < 256; X++)
                {
                    float Peak = 0;
                    int B1 = (int)Math.Pow(2, X * 10.0 / (256 - 1));
                    if (B1 > 1023) B1 = 1023;
                    if (B1 <= B0) B1 = B0 + 1;
                    for (; B0 < B1; B0++)
                    {
                        if (Peak < AudioData[1 + B0]) Peak = AudioData[1 + B0];
                    }
                    Y = (double)(Math.Sqrt(Peak) * (1024 - 1) - 4);
                    if (Y > 1024)
                        Y = 1024;
                    if (Y < 0)
                        Y = 0;

                    AudioDataPointStore[X].Add(Y);
                    while (AudioDataPointStore[X].Count > 100)
                        AudioDataPointStore[X].RemoveAt(0);
                }

                for (int i = 0; i < _VisualizerStack.Count; i++)
                {
                    _VisualizerStack[i].UpdateVisualizer(AudioDataPointStore);
                }

                VisualizerUpdatesCounter++;
                if ((DateTime.Now - VisualizerRPSCounter).TotalSeconds >= 1)
                {
                    Dispatcher.Invoke(() =>
                    {
                        RPSLabel.Content = "RPS: " + VisualizerUpdatesCounter.ToString();
                    });
                    VisualizerUpdatesCounter = 0;
                    VisualizerRPSCounter = DateTime.Now;
                }

                CalibrateRefreshRate.Stop();
                int ActuralRefreshTime = RefreshRate - (int)CalibrateRefreshRate.Elapsed.Milliseconds;

                if (ActuralRefreshTime < 0)
                    ActuralRefreshTime = 0;

                Thread.Sleep(ActuralRefreshTime);
            }
            VisualizersRunning = false;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        public void RefreshVisualizerCountLabel()
        {
            ActiveVisualizerLabel.Content = "Currently " + VisualizerStack.Count.ToString() + " Visualizers active";
        }

        private void LoadConfigButton_Click(object sender, RoutedEventArgs e)
        {
            LoadFileDialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Saves";
            if (LoadFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadConfig(LoadFileDialog.FileName);
            }
        }

        private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Saves";
            if (SaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveConfig(SaveFileDialog.FileName);
            }
        }

        async void LoadConfig(string FileName)
        {
            FI SaveConfigs = new FI();
            SaveConfigs.LoadFile(FileName);
            FI.CategoryData CurrentCategory = new FI.CategoryData("MainWindow Data", 0);
            this.Left = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "Location X");
            this.Top = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "Location Y");
            AudioSourceCombobox.SelectedIndex = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "Audio Source Index");
            RefreshRateCombobox.SelectedIndex = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "Refresh rate Index");

            var tasks = new List<Task>();

            int Count = VisualizerStack.Count;
            for (int i = 0; i < Count; i++)
            {
                var task = VisualizerStack[i].PerformClose();
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            CurrentCategory = new FI.CategoryData("Visualizer Window", 0);
            while (SaveConfigs.IsAnyMoreInCat(CurrentCategory))
            {
                VisualizerControl NewWindow = new VisualizerControl(this);
                NewWindow.Left = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "Location X");
                NewWindow.Top = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "Location Y");
                NewWindow.Width = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "Width");
                NewWindow.Height = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "Height");
                NewWindow.VisualSamples = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "Visual Samples");
                NewWindow.Smoothness = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "Smoothness");
                NewWindow.Sensitivity = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "Sensitivity");
                NewWindow.BeatZoneFrom = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "BeatZone From");
                NewWindow.BeatZoneTo = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "BeatZone To");
                NewWindow.VisualizationIndex = SaveConfigs.FindtItemInItemstack_INT(CurrentCategory, "Visualization type index");
                NewWindow.Forecolor = SaveConfigs.FindtItemInItemstack_BRS(CurrentCategory, "Forecolor");
                NewWindow.BackColor = SaveConfigs.FindtItemInItemstack_BRS(CurrentCategory, "Backcolor");
                NewWindow.TopMost = SaveConfigs.FindtItemInItemstack_BOL(CurrentCategory, "TopMost");

                NewWindow.Initialize();

                VisualizerStack.Add(NewWindow);
                NewWindow.Show();
                CurrentCategory.Offset++;
            }
        }

        void SaveConfig(string FileName)
        {
            FI SaveConfigs = new FI();
            SaveConfigs.ItemStack.Add(
                new FI.ValueCategory("MainWindow Data", new List<FI.FIItems>() {
                    new FI.IntValue("Location X", (int)this.Left),
                    new FI.IntValue("Location Y", (int)this.Top),
                    new FI.IntValue("Audio Source Index", AudioSourceCombobox.SelectedIndex),
                    new FI.IntValue("Refresh rate Index", RefreshRateCombobox.SelectedIndex)
                }));

            for (int i = 0; i < VisualizerStack.Count; i++)
            {
                SaveConfigs.ItemStack.Add(
                new FI.ValueCategory("Visualizer Window", new List<FI.FIItems>() {
                    new FI.IntValue("Location X", (int)VisualizerStack[i].Left),
                    new FI.IntValue("Location Y", (int)VisualizerStack[i].Top),
                    new FI.IntValue("Width", (int)VisualizerStack[i].Width),
                    new FI.IntValue("Height", (int)VisualizerStack[i].Height),
                    new FI.IntValue("Visual Samples", VisualizerStack[i].VisualSamples),
                    new FI.IntValue("Smoothness", VisualizerStack[i].Smoothness),
                    new FI.IntValue("Sensitivity", VisualizerStack[i].Sensitivity),
                    new FI.IntValue("BeatZone From", VisualizerStack[i].BeatZoneFrom),
                    new FI.IntValue("BeatZone To", VisualizerStack[i].BeatZoneTo),
                    new FI.IntValue("Visualization type index", VisualizerStack[i].VisualizationIndex),
                    new FI.BrushValue("Forecolor", VisualizerStack[i].Forecolor),
                    new FI.BrushValue("Backcolor", VisualizerStack[i].BackColor),
                    new FI.BoolValue("TopMost", VisualizerStack[i].TopMost)
                }));
            }

            SaveConfigs.SaveFile(FileName);
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ProperClosed)
            {
                e.Cancel = true;

                RunVisualizer = false;

                SaveConfig("Saves\\autosave.txt");

                var tasks = new List<Task>();

                int Count = VisualizerStack.Count;
                for (int i = 0; i < Count; i++)
                {
                    var task = VisualizerStack[i].PerformClose();
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                VisualizerStack.Clear();

                await GI.FadeOut(this);

                while (VisualizersRunning)
                    await Task.Delay(100);

                ProperClosed = true;

                Application.Current.Shutdown();
            }
        }
    }
}
