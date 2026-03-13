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

        public void UpdateTimeDisplay(TimeSpan time)
        {
            TimeLabel.Text = time.ToString(@"hh\:mm\:ss");
        }

        private void ResetTimer(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App app)
            {
                app.ResetTimer();
            }
        }
    }
}