using System;
using System.ComponentModel;
using System.Windows;

namespace DesktopTimeTracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
    }
}