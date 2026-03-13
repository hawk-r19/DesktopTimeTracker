using System;
using System.Windows;
using System.Windows.Controls;

namespace DesktopTimeTracker
{
    public partial class TrayPopup : UserControl
    {
        public TrayPopup()
        {
            InitializeComponent();
        }

        public void UpdateTimer(TimeSpan time, bool isRunning, bool paused)
        {
            // Update timer display
            if (time.TotalHours >= 24)
            {
                TimerDisplay.Text = $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            }
            else
            {
                TimerDisplay.Text = time.ToString(@"hh\:mm\:ss");
            }

            // Update status
            StatusText.Text = (paused ? "Paused, " : "") + 
                            (isRunning ? "On Task" : "Off Task");
            StatusText.Foreground = isRunning && !paused ? 
                System.Windows.Media.Brushes.Green : 
                System.Windows.Media.Brushes.Gray;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App app)
            {
                app.ResetTimer();
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App app)
            {
                app.TogglePauseTimer();
                if (app.isTimerPaused())
                {
                    PauseButton.Content = "Resume";
                }
                else
                {
                    PauseButton.Content = "Pause";
                }
            }
        }

        private void OpenWindow_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App app)
            {
                app.OpenMainWindow();
            }
        }
    }
}
