using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopTimeTracker
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<DesktopButton> DesktopButtons { get; set; }
        private string activeColor = "Green";
        private string inactiveColor = "#555555";

        public MainWindow()
        {
            InitializeComponent();
            makeDesktopButtons(VirtualDesktop.GetDesktopCount());
            DataContext = this;
        }

        private void makeDesktopButtons(int desktops)
        {
            // between 1 and 5 desktops, to avoid UI issues with too many buttons
            desktops = Math.Min(Math.Max(1, desktops), 5);
            DesktopButtons = new ObservableCollection<DesktopButton> {};
            for (int i = 0; i < desktops; i++)
            {
                DesktopButtons.Add(new DesktopButton { Id = i, Name = $"{i+1}", statusColor = inactiveColor });
            }
            // set state of active desktop buttons
            if (Application.Current is App app)
            {
                bool[] activeDesktops = app.getActiveDesktops();
                foreach (var button in DesktopButtons)
                {
                    if (activeDesktops[button.Id])
                    {
                        button.statusColor = activeColor;
                    }
                }
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        // Override the closing event to hide the window instead of closing it
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }   

        public void UpdateTimeDisplay(TimeSpan time, bool isRunning, bool paused)
        {
            // Use different format that handles days if time exceeds 24 hours
            if (time.TotalHours >= 24)
            {
                TimeLabel.Text = $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            }
            else
            {
                TimeLabel.Text = time.ToString(@"hh\:mm\:ss");
            }

            // Update status
            StatusText.Text = paused ? "Paused" : 
                            isRunning ? "On Task" : "Off Task";
            StatusText.Foreground = isRunning && !paused ?
                System.Windows.Media.Brushes.Green :
                System.Windows.Media.Brushes.Gray;
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App app)
            {
                app.TogglePauseTimer();
                if (app.isTimerPaused())
                {
                    PauseButton.Content = "Resume Tracking";
                }
                else
                {
                    PauseButton.Content = "Pause Tracking";
                }
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App app)
            {
                app.ResetTimer();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App app)
            {
                app.Shutdown();
            }
        }

        private void Desktop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && Application.Current is App app)
            {
                app.toggleDesktopTracking((int)button.Tag);
                string color = app.isTargetDesktop((int)button.Tag) ? activeColor : inactiveColor;
                button.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            }
        }

        public class DesktopButton
        {
            public required int Id { get; set; }
            public required string Name { get; set; }
            public required string statusColor { get; set; }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}