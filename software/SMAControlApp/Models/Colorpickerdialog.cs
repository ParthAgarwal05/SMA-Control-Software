using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SMAControlApp.Views
{
    /// <summary>
    /// Simple color picker dialog – no third-party packages needed.
    /// </summary>
    public class ColorPickerDialog : Window
    {
        public Color SelectedColor { get; private set; }

        private static readonly Color[] Presets =
        {
            Colors.DeepSkyBlue,   Colors.DodgerBlue,    Colors.SteelBlue,
            Colors.OrangeRed,     Colors.Coral,         Colors.Tomato,
            Colors.LimeGreen,     Colors.MediumSeaGreen,Colors.Aquamarine,
            Colors.Gold,          Colors.Yellow,        Colors.Orange,
            Colors.MediumOrchid,  Colors.Violet,        Colors.HotPink,
            Colors.Cyan,          Colors.White,         Colors.Silver
        };

        public ColorPickerDialog(Color currentColor)
        {
            Title = "Choose Sensor Color";
            Width = 260;
            Height = 210;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            SelectedColor = currentColor;

            var wrap = new WrapPanel { Margin = new Thickness(10) };

            foreach (var color in Presets)
            {
                var c = color;
                var border = new Border
                {
                    Width = 36,
                    Height = 36,
                    Margin = new Thickness(3),
                    CornerRadius = new CornerRadius(4),
                    Background = new SolidColorBrush(c),
                    BorderBrush = c == currentColor ? Brushes.Black : Brushes.Transparent,
                    BorderThickness = new Thickness(2),
                    Cursor = Cursors.Hand,
                    ToolTip = c.ToString()
                };
                border.MouseLeftButtonUp += (_, _) => { SelectedColor = c; DialogResult = true; };
                wrap.Children.Add(border);
            }

            Content = new ScrollViewer
            {
                Content = wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }
    }
}