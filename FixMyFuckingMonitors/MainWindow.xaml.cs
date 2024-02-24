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
//using PInvoke;
using System;
using System.Runtime.InteropServices;
using static LibMyFuckingMonitors.MonitorService;
using LibMyFuckingMonitors;
using System.Windows.Ink;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Controls.Ribbon.Primitives;

namespace FixMyFuckingMonitors
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<MonitorModel> Monitors { get; set; } = new List<MonitorModel>();
        public static int Zoom = 10;
        public static int SelectedMonitor = -1;
        public MainWindow()
        {
            InitializeComponent();
            RefreshCanvas();
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var local = (Grid)sender;
            var monitorIndex = int.Parse(local.Name.Replace("mon_", string.Empty));
            if (monitorIndex == SelectedMonitor)
            {
                monitorIndex = -1;
                SelectedMonitor = -1;

            }
            SelectMonitor(monitorIndex);
        }

        public void SelectMonitor(int monitorIndex)
        {
            Grid local = null;
            foreach (Grid grid in MonitorCanvas.Children)
            {
                var temp = (System.Windows.Shapes.Rectangle)(grid.Children[0]);
                temp.Fill = new SolidColorBrush(Colors.DarkGray);
                if (grid.Name == $"mon_{monitorIndex.ToString()}")
                {
                    local = grid;
                }
            }
            if (local != null)
            {
                var rect = (System.Windows.Shapes.Rectangle)local.Children[0];
                rect.Fill = new SolidColorBrush(Colors.Green);
                //load values
                var monitor = Monitors[monitorIndex];
                SelectedMonitor = monitorIndex;
                LoadMonitorData(monitor);
            }
            else
                LoadMonitorData(new MonitorModel() { MonNum = SelectedMonitor });
        }

        public void LoadMonitorData(MonitorModel monitor)
        {
            modesCombo.Items.Clear();
            if (SelectedMonitor >= 0)
            {
                MonNameLabel.Content = "Name: " + monitor.Description;
                xPosInput.Text = monitor.Xpos.ToString();
                yPosInput.Text = monitor.Ypos.ToString();
                foreach (var mode in monitor.Modes)
                {
                    modesCombo.Items.Add($"{mode.Width} by {mode.Height} @{mode.Frequency}hz ColorBits: {mode.ColorBits}");
                }
                modesCombo.SelectedItem = $"{monitor.Width} by {monitor.Height} @{monitor.Frequency}hz ColorBits: {monitor.ColorBits}";
            }
            else
            {
                MonNameLabel.Content = string.Empty;
            }
        }

        public int posOffset(int canv, int pos) => (int)canv / 2 + pos;

        public void RefreshCanvas()
        {
            MonitorCanvas.Children.Clear();
            Monitors = GetMonitorOffsets();
            var totalWidth = Monitors[0].Width;
            var totalHeight = Monitors[0].Height;
            var xOffset = 0;
            var yOffset = 0;
            foreach (var monitor in Monitors)
            {
                totalWidth += Math.Abs(monitor.Xpos);
                totalHeight += Math.Abs(monitor.Ypos);
                if (monitor.Xpos < 0)
                    xOffset += Math.Abs(monitor.Xpos);
                if (monitor.Ypos < 0)
                    yOffset += Math.Abs(monitor.Ypos);
            }
            foreach (var monitor in Monitors)
            {
                System.Windows.Shapes.Rectangle rect;
                rect = new System.Windows.Shapes.Rectangle();
                rect.Stroke = new SolidColorBrush(Colors.Black);
                rect.Fill = new SolidColorBrush(Colors.DarkGray);
                rect.Width = Convert.ToDouble(monitor.Width / Zoom);
                rect.Height = Convert.ToDouble(monitor.Height / Zoom);

                Grid grid = new Grid();
                var prePosX = monitor.Xpos - totalWidth/2;
                var prePosY = monitor.Ypos - totalHeight/2;
                var baseWidth = Convert.ToDouble((prePosX+xOffset) / Zoom);
                var baseHeight = Convert.ToDouble((prePosY+yOffset) / Zoom);
                var canvWidth = (MonitorCanvas.ActualWidth / 2);
                var canvHeight = (MonitorCanvas.ActualHeight / 2);

                var xPos = baseWidth+canvWidth;
                var yPos = baseHeight+canvHeight;

                Canvas.SetLeft(grid, xPos);
                Canvas.SetTop(grid, yPos);

                grid.Children.Add(rect);
                TextBlock textblock = new TextBlock();
                var primaryString = monitor.Primary ? "PRIMARY\r\n" : String.Empty;
                textblock.Text = $"{primaryString}Name: {monitor.Description}\r\n" + (monitor.MonNum + 1).ToString() + $": {monitor.Width}X{monitor.Height}@{monitor.Frequency}hz\r\nX: {monitor.Xpos} Y: {monitor.Ypos}\r\nGPU: {monitor.GPU}";
                textblock.HorizontalAlignment = HorizontalAlignment.Center;
                textblock.VerticalAlignment = VerticalAlignment.Center;
                textblock.FontSize = 120/Zoom;
                grid.Children.Add(textblock);
                grid.Name = "mon_" + monitor.MonNum.ToString();
                grid.MouseUp += Grid_MouseUp;
                MonitorCanvas.Children.Add(grid);
            }
            if (SelectedMonitor >= 0)
            {
                SelectMonitor(SelectedMonitor);
            }
        }

        //helpers
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        //events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshCanvas();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshCanvas();
        }

        private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Zoom = (int)e.NewValue;
            RefreshCanvas();
        }

        private void MonitorCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                zoomSlider.Value++;

            else if (e.Delta < 0)
                zoomSlider.Value--;
        }

        private void numeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void setbutton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMonitor >= 0)
            {
                var monitor = Monitors[SelectedMonitor];
                if (monitor.Primary && (int.Parse(xPosInput.Text) != 0 || int.Parse(yPosInput.Text) != 0))
                {
                    MonNameLabel.Content = "Primary monitor is always at 0x0 offset. Can not update position.";
                    return;
                }
                var selectedMode = modesCombo.SelectedValue.ToString();
                var modeValsRaw = selectedMode.Split(' ');
                //$"{monitor.Width} by {monitor.Height} @{monitor.Frequency}hz ColorBits: {monitor.ColorBits}";
                var newMode = new MonitorModes() { 
                    Width = int.Parse(modeValsRaw[0]),
                    Height = int.Parse(modeValsRaw[2]),
                    Frequency = int.Parse(modeValsRaw[3].Replace("@",string.Empty).Replace("hz", string.Empty)),
                    ColorBits = int.Parse(modeValsRaw[5])
                };
                SetMonitorOffsets(monitor.InternalId, int.Parse(xPosInput.Text), int.Parse(yPosInput.Text));
                SetMonitorMode(monitor.InternalId, newMode);
            }
            else
                MonNameLabel.Content = "No monitor was selected, numbnuts";
            RefreshCanvas();
        }

        private void modesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set new mode here
        }
    }
}
