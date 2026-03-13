using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace DesktopTimeTracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /*protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }*/

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