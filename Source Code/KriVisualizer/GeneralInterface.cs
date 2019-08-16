using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace KriVisualizer
{
    // General Interface
    class GI
    {
        public struct VisualizationModeColors
        {
            public Brush BorderColor;
            public Brush FillColor;

            public VisualizationModeColors(Brush _BorderColor, Brush _FillColor)
            {
                BorderColor = _BorderColor;
                FillColor = _FillColor;
            }
        }

        public static readonly string[] VisualizationModesString = { "-- Select a Mode --", "Line", "Column", "3D Column", "Expansive Column" };

        public static async Task FadeIn(UIElement FadeElement)
        {
            for (int i = 0; i <= 100; i += 5)
            {
                FadeElement.Opacity = ((double)i / (double)100);
                await Task.Delay(10);
            }
            FadeElement.Opacity = 1;
        }

        public static async Task FadeOut(UIElement FadeElement)
        {
            for (int i = 100; i >= 0; i -= 5)
            {
                FadeElement.Opacity = ((double)i / (double)100);
                await Task.Delay(10);
            }
            FadeElement.Opacity = 0;
        }

        public static void GenerateVisualization(int VisualizationIndex, Canvas SenderCanvas, List<double> AudioValues, double _Width, double _Height, VisualizationModeColors _ColorData)
        {
            double TransformX = _Width / AudioValues.Count;
            double TransformY = _Height / 1024;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (var draw = drawingVisual.RenderOpen())
            {
                for (int i = 0; i < AudioValues.Count; i++)
                {
                    if (VisualizationIndex == 1)
                    {
                        if (i == 0)
                        {
                            draw.DrawLine(new Pen(_ColorData.BorderColor, 3), new Point(0, _Height), new Point(TransformX * i, _Height - (TransformY * AudioValues[i])));
                        }
                        else
                        {
                            draw.DrawLine(new Pen(_ColorData.BorderColor, 3), new Point(TransformX * (i - 1), _Height - (TransformY * AudioValues[i - 1])), new Point(TransformX * i, _Height - (TransformY * AudioValues[i])));
                        }
                    }
                    if (VisualizationIndex == 2)
                    {
                        if (i == 0)
                        {
                            draw.DrawRectangle(_ColorData.FillColor, new Pen(_ColorData.BorderColor, 3), new Rect(
                                0,
                                _Height - (TransformY * AudioValues[i]),
                                TransformX,
                                _Height - (_Height - (TransformY * AudioValues[i]))
                            ));
                        }
                        else
                        {
                            draw.DrawRectangle(_ColorData.FillColor, new Pen(_ColorData.BorderColor, 3), new Rect(
                                TransformX * (i - 1),
                                _Height - (TransformY * AudioValues[i]),
                                TransformX,
                                _Height - (_Height - (TransformY * AudioValues[i]))
                            ));
                        }
                    }
                    if (VisualizationIndex == 3)
                    {
                        if (i == 0)
                        {
                            draw.DrawRectangle(_ColorData.FillColor, new Pen(_ColorData.BorderColor, 3), new Rect(
                                0,
                                _Height - (TransformY * AudioValues[i]),
                                TransformX * 4,
                                _Height - (_Height - (TransformY * AudioValues[i]))
                            ));
                        }
                        else
                        {
                            draw.DrawRectangle(_ColorData.FillColor, new Pen(_ColorData.BorderColor, 3), new Rect(
                                TransformX * (i - 1),
                                _Height - (TransformY * AudioValues[i]),
                                TransformX * 4,
                                _Height - (_Height - (TransformY * AudioValues[i]))
                            ));
                        }
                    }
                    if (VisualizationIndex == 4)
                    {
                        if (i == 0)
                        {
                            draw.DrawRectangle(_ColorData.FillColor, new Pen(_ColorData.BorderColor, 3), new Rect(
                                0,
                                _Height - (TransformY * AudioValues[i]),
                                (_Width - TransformX * i) * ((TransformY * AudioValues[i]) / _Height),
                                _Height - (_Height - (TransformY * AudioValues[i]))
                            ));
                        }
                        else
                        {
                            draw.DrawRectangle(_ColorData.FillColor, new Pen(_ColorData.BorderColor, 3), new Rect(
                                TransformX * (i - 1),
                                _Height - (TransformY * AudioValues[i]),
                                (_Width - TransformX * i) * ((TransformY * AudioValues[i]) / _Height),
                                _Height - (_Height - (TransformY * AudioValues[i]))
                            ));
                        }
                    }
                }
            }

            SenderCanvas.Children.Clear();
            SenderCanvas.Children.Add(new VisualHost { Visual = drawingVisual });
        }
    }
    public class VisualHost : UIElement
    {
        public Visual Visual { get; set; }

        protected override int VisualChildrenCount
        {
            get { return Visual != null ? 1 : 0; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return Visual;
        }
    }
}
