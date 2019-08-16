using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace KriVisualizer
{
    /// <summary>
    /// Interaction logic for VisualizerControl.xaml
    /// </summary>
    public partial class VisualizerControl : Window
    {
        MainWindow SenderWindow;
        bool SuspendUpdate = false;

        bool Moving = false;
        bool DragingTop = false;
        bool DragingLeft = false;
        bool DragingBottom = false;
        bool DragingRight = false;
        double WidthStandpoint = 0;
        double HeightStandpoint = 0;
        Point StartDragPoint = new Point();
        Point StartDragPointTotal = new Point();
        GI.VisualizationModeColors ColorData = new GI.VisualizationModeColors(Brushes.White, Brushes.White);

        bool VisualSamplesInvokeRequiered = false;
        bool SmoothnessInvokeRequiered = false;
        bool SensitivityInvokeRequiered = false;
        bool BeatZoneFromInvokeRequiered = false;
        bool BeatZoneToInvokeRequiered = false;
        bool VisualizationIndexInvokeRequiered = false;

        public int VisualSamples = 128;
        public int Smoothness = 0;
        public int Sensitivity = 0;
        public int BeatZoneFrom = 0;
        public int BeatZoneTo = 128;
        public int VisualizationIndex = 0;
        public Brush Forecolor = Brushes.White;
        public Brush BackColor = Brushes.White;
        public bool TopMost = false;

        public VisualizerControl(MainWindow _SenderWindow)
        {
            SenderWindow = _SenderWindow;
            InitializeComponent();

            for (int i = 0; i < GI.VisualizationModesString.Length; i++)
                VisualizerControlTypeCombobox.Items.Add(GI.VisualizationModesString[i]);
        }

        public void Initialize()
        {
            VisualizerControlVisualSamplesSlider.Value = VisualSamples;
            VisualizerControlSmoothnessSlider.Value = Smoothness;
            VisualizerControlSensitivitySlider.Value = Sensitivity;
            VisualizerControlBeatZoneFromSlider.Value = BeatZoneFrom;
            VisualizerControlBeatZoneToSlider.Value = BeatZoneTo;
            VisualizerControlTypeCombobox.SelectedIndex = VisualizationIndex;
            ColorData.BorderColor = Forecolor;
            VisualizerControlBorderColorButton.Background = Forecolor;
            ColorData.FillColor = BackColor;
            VisualizerControlFillColorButton.Background = BackColor;
            VisualizerControlTopMostCheckbox.IsChecked = TopMost;
            Topmost = TopMost;
        }

        private void VisualizerControlDragBars_MouseDown(object sender, MouseButtonEventArgs e)
        {
            VisualizerControlBeatZoneCanvas.Children.Clear();
            SuspendUpdate = true;
            if (!Moving)
            {
                if ((sender as Canvas).Name == VisualizerControlTopDragbar.Name)
                    DragingTop = true;
                if ((sender as Canvas).Name == VisualizerControlLeftDragbar.Name)
                    DragingLeft = true;
                if ((sender as Canvas).Name == VisualizerControlBottomDragbar.Name)
                    DragingBottom = true;
                if ((sender as Canvas).Name == VisualizerControlRightDragbar.Name)
                    DragingRight = true;

                if ((sender as Canvas).Name == VisualizerControlTopLeftDragbar.Name)
                {
                    DragingTop = true;
                    DragingLeft = true;
                }
                if ((sender as Canvas).Name == VisualizerControlTopRightDragbar.Name)
                {
                    DragingTop = true;
                    DragingRight = true;
                }
                if ((sender as Canvas).Name == VisualizerControlBottomLeftDragbar.Name)
                {
                    DragingLeft = true;
                    DragingBottom = true;
                }
                if ((sender as Canvas).Name == VisualizerControlBottomRightDragbar.Name)
                {
                    DragingRight = true;
                    DragingBottom = true;
                }

                WidthStandpoint = this.Width;
                HeightStandpoint = this.Height;

                Mouse.Capture(sender as Canvas);
                StartDragPoint = e.GetPosition(this);
                StartDragPointTotal = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            }
        }

        private void VisualizerControlDragBars_MouseMove(object sender, MouseEventArgs e)
        {
            if (DragingTop)
            {
                if (HeightStandpoint - (System.Windows.Forms.Cursor.Position.Y - StartDragPointTotal.Y) > this.MinHeight)
                {
                    this.Height = HeightStandpoint - (System.Windows.Forms.Cursor.Position.Y - StartDragPointTotal.Y);
                    this.Top = System.Windows.Forms.Cursor.Position.Y;
                }
            }
            if (DragingLeft)
            {
                if (WidthStandpoint - (System.Windows.Forms.Cursor.Position.X - StartDragPointTotal.X) > this.MinWidth)
                {
                    this.Width = WidthStandpoint - (System.Windows.Forms.Cursor.Position.X - StartDragPointTotal.X);
                    this.Left = System.Windows.Forms.Cursor.Position.X;
                }
            }
            if (DragingBottom)
            {
                if (HeightStandpoint + (System.Windows.Forms.Cursor.Position.Y - StartDragPointTotal.Y) > this.MinHeight)
                {
                    this.Height = HeightStandpoint + (System.Windows.Forms.Cursor.Position.Y - StartDragPointTotal.Y);
                }
            }
            if (DragingRight)
            {
                if (WidthStandpoint + (System.Windows.Forms.Cursor.Position.X - StartDragPointTotal.X) > this.MinWidth)
                {
                    this.Width = WidthStandpoint + (System.Windows.Forms.Cursor.Position.X - StartDragPointTotal.X);
                }
            }
        }

        private void VisualizerControlDragBars_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DragingTop = false;
            DragingLeft = false;
            DragingBottom = false;
            DragingRight = false;
            SuspendUpdate = false;
            Mouse.Capture(null);
        }

        private void VisualizerControlDragButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Moving = true;
            Mouse.Capture(VisualizerControlDragButton);
            StartDragPoint = e.GetPosition(this);
        }

        private void VisualizerControlDragButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (Moving)
            {
                this.Left = (System.Windows.Forms.Cursor.Position.X - StartDragPoint.X);
                this.Top = (System.Windows.Forms.Cursor.Position.Y - StartDragPoint.Y);
            }
        }

        private void VisualizerControlDragButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Moving = false;
            Mouse.Capture(null);
        }

        public void HideInterface()
        {
            VisualizerControlTopDragbar.Visibility = Visibility.Hidden;
            VisualizerControlLeftDragbar.Visibility = Visibility.Hidden;
            VisualizerControlBottomDragbar.Visibility = Visibility.Hidden;
            VisualizerControlRightDragbar.Visibility = Visibility.Hidden;
            VisualizerControlTopLeftDragbar.Visibility = Visibility.Hidden;
            VisualizerControlTopRightDragbar.Visibility = Visibility.Hidden;
            VisualizerControlBottomLeftDragbar.Visibility = Visibility.Hidden;
            VisualizerControlBottomRightDragbar.Visibility = Visibility.Hidden;
            VisualizerControlDragButton.Visibility = Visibility.Hidden;
            VisualizerControlSettingsGrid.Visibility = Visibility.Hidden;
        }

        public void ShowInterface()
        {
            VisualizerControlTopDragbar.Visibility = Visibility.Visible;
            VisualizerControlLeftDragbar.Visibility = Visibility.Visible;
            VisualizerControlBottomDragbar.Visibility = Visibility.Visible;
            VisualizerControlRightDragbar.Visibility = Visibility.Visible;
            VisualizerControlTopLeftDragbar.Visibility = Visibility.Visible;
            VisualizerControlTopRightDragbar.Visibility = Visibility.Visible;
            VisualizerControlBottomLeftDragbar.Visibility = Visibility.Visible;
            VisualizerControlBottomRightDragbar.Visibility = Visibility.Visible;
            VisualizerControlDragButton.Visibility = Visibility.Visible;
            VisualizerControlSettingsGrid.Visibility = Visibility.Visible;
        }

        private async void VisualizerControlRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformClose();
        }

        public async Task PerformClose()
        {
            await GI.FadeOut(this);
            SenderWindow.VisualizerStack.Remove(this);
            SenderWindow.RefreshVisualizerCountLabel();
            this.Close();
        }

        public void UpdateVisualizer(List<List<double>> _AudioDataPointStore)
        {
            if (!SuspendUpdate)
            {
                if (VisualSamplesInvokeRequiered)
                    Dispatcher.Invoke(() => { VisualSamples = (int)VisualizerControlVisualSamplesSlider.Value; });
                if (SmoothnessInvokeRequiered)
                    Dispatcher.Invoke(() => { Smoothness = (int)VisualizerControlSmoothnessSlider.Value; });
                if (SensitivityInvokeRequiered)
                    Dispatcher.Invoke(() => { Sensitivity = (int)VisualizerControlSensitivitySlider.Value; });
                if (BeatZoneFromInvokeRequiered)
                    Dispatcher.Invoke(() => { BeatZoneFrom = (int)VisualizerControlBeatZoneFromSlider.Value; });
                if (BeatZoneToInvokeRequiered)
                    Dispatcher.Invoke(() => { BeatZoneTo = (int)VisualizerControlBeatZoneToSlider.Value; });
                if (VisualizationIndexInvokeRequiered)
                    Dispatcher.Invoke(() => { VisualizationIndex = (int)VisualizerControlTypeCombobox.SelectedIndex; });

                VisualSamplesInvokeRequiered = false;
                SmoothnessInvokeRequiered = false;
                SensitivityInvokeRequiered = false;
                BeatZoneFromInvokeRequiered = false;
                BeatZoneToInvokeRequiered = false;
                VisualizationIndexInvokeRequiered = false;

                List<double> AudioValues = new List<double>(new double[BeatZoneTo - BeatZoneFrom]);
                int AudioIndex = 0;
                int ActIndex = 0;

                for (double j = 0; j < _AudioDataPointStore.Count; j += (double)_AudioDataPointStore.Count / VisualSamples)
                {
                    int ActJ = (int)Math.Round(j,0);
                    if (ActJ >= _AudioDataPointStore.Count)
                        break;

                    if (ActIndex > BeatZoneFrom && ActIndex < BeatZoneTo)
                    {
                        if (Smoothness != 1)
                        {
                            double AvrVal = 0;
                            for (int i = _AudioDataPointStore[ActJ].Count - 1; i >= _AudioDataPointStore[ActJ].Count - Smoothness; i--)
                            {
                                AvrVal += (_AudioDataPointStore[ActJ][i] * Sensitivity);
                            }
                            AvrVal = AvrVal / Smoothness;
                            if (AvrVal > 1024)
                                AvrVal = 1024;
                            if (AvrVal < 0)
                                AvrVal = 0;
                            AudioValues[AudioIndex] = AvrVal;
                        }
                        else
                        {
                            AudioValues[AudioIndex] = _AudioDataPointStore[ActJ][_AudioDataPointStore[ActJ].Count - 1] * Sensitivity;
                        }
                        AudioIndex++;
                    }
                    ActIndex++;
                }

                Dispatcher.Invoke(() => {
                    GI.GenerateVisualization(VisualizationIndex, VisualizerControlBeatZoneCanvas, AudioValues, VisualizerControlBeatZoneCanvas.ActualWidth, VisualizerControlBeatZoneCanvas.ActualHeight, ColorData);
                });
            }
        }

        private void VisualizerControlVisualSamplesSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SenderWindow.VisualizersRunning)
            {
                VisualSamplesInvokeRequiered = true;
            }
            else
            {
                VisualSamples = (int)(sender as Slider).Value;
            }

            if (VisualizerControlVisualSamplesValueLabel != null)
                VisualizerControlVisualSamplesValueLabel.Content = (Math.Round((sender as Slider).Value, 0)).ToString();

            if (VisualizerControlBeatZoneFromSlider != null)
            {
                VisualizerControlBeatZoneFromSlider.Maximum = (sender as Slider).Value - 1;
                VisualizerControlBeatZoneFromSlider.Value = 0;
            }
            if (VisualizerControlBeatZoneToSlider != null)
            {
                VisualizerControlBeatZoneToSlider.Maximum = (sender as Slider).Value;
                VisualizerControlBeatZoneToSlider.Value = (sender as Slider).Value;
            }
        }

        private void VisualizerControlSmoothnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SenderWindow.VisualizersRunning)
            {
                SmoothnessInvokeRequiered = true;
            }
            else
            {
                Smoothness = (int)(sender as Slider).Value;
            }

            if (VisualizerControlSmoothnessValueLabel != null)
                VisualizerControlSmoothnessValueLabel.Content = (Math.Round((sender as Slider).Value, 0)).ToString();
        }

        private void VisualizerControlSensitivitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SenderWindow.VisualizersRunning)
            {
                SensitivityInvokeRequiered = true;
            }
            else
            {
                Sensitivity = (int)(sender as Slider).Value;
            }

            if (VisualizerControlSensitivityValueLabel != null)
                VisualizerControlSensitivityValueLabel.Content = (Math.Round((sender as Slider).Value, 0)).ToString();
        }

        private void VisualizerControlBeatZoneFromSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SenderWindow.VisualizersRunning)
            {
                BeatZoneFromInvokeRequiered = true;
            }
            else
            {
                BeatZoneFrom = (int)(sender as Slider).Value;
            }

            if (VisualizerControlBeatZoneFromValueLabel != null)
                VisualizerControlBeatZoneFromValueLabel.Content = (Math.Round((sender as Slider).Value, 0)).ToString();
        }

        private void VisualizerControlBeatZoneToSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SenderWindow.VisualizersRunning)
            {
                BeatZoneToInvokeRequiered = true;
            }
            else
            {
                BeatZoneTo = (int)(sender as Slider).Value;
            }

            if (VisualizerControlBeatZoneToValueLabel != null)
                VisualizerControlBeatZoneToValueLabel.Content = (Math.Round((sender as Slider).Value, 0)).ToString();
        }

        private void VisualizerControlTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SenderWindow.VisualizersRunning)
            {
                VisualizationIndexInvokeRequiered = true;
            }
            else
            {
                VisualizationIndex = VisualizerControlTypeCombobox.SelectedIndex;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await GI.FadeIn(this);
        }

        private async void VisualizerControlBorderColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (TopMost == true)
                Topmost = false;
            ColorSelector PickColor = new ColorSelector(new Point(this.Left, this.Top));
            PickColor.Show();
            while (true)
            {
                if (PickColor.SelectionMade)
                {
                    if (PickColor.SelectedColor != null)
                    {
                        ColorData.BorderColor = PickColor.SelectedColor;
                        VisualizerControlBorderColorButton.Background = PickColor.SelectedColor;
                        Forecolor = PickColor.SelectedColor;
                    }

                    PickColor.Close();
                    break;
                }
                await Task.Delay(100);
            }
            if (TopMost == true)
                Topmost = true;
        }

        private async void VisualizerControlFillColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (TopMost == true)
                Topmost = false;
            ColorSelector PickColor = new ColorSelector(new Point(this.Left, this.Top));
            PickColor.Show();
            while (true)
            {
                if (PickColor.SelectionMade)
                {
                    if (PickColor.SelectedColor != null)
                    {
                        ColorData.FillColor= PickColor.SelectedColor;
                        VisualizerControlFillColorButton.Background = PickColor.SelectedColor;
                        BackColor = PickColor.SelectedColor;
                    }

                    PickColor.Close();
                    break;
                }
                await Task.Delay(100);
            }
            if (TopMost == true)
                Topmost = true;
        }

        private void VisualizerControlTopMostCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            TopMost = true;
        }

        private void VisualizerControlTopMostCheckbox_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
            TopMost = false;
        }
    }
}
