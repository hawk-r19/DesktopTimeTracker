using Hardcodet.Wpf.TaskbarNotification;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace DesktopTimeTracker
{
    public partial class App : Application
    {
        private TaskbarIcon? notifyIcon;
        private DispatcherTimer? timer;
        private TimeSpan elapsedTime;
        private int desktop = 3;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize the system tray icon
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            
            // Initialize and start the timer
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            elapsedTime = TimeSpan.Zero;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (checkDesktop() != desktop)
            {
                return;
            }

            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
            
            // Update MainWindow if it exists and is loaded
            if (Current.MainWindow is MainWindow mainWindow && mainWindow.IsLoaded)
            {
                mainWindow.UpdateTimeDisplay(elapsedTime);
            }
        }

        [DllImport("VirtualDesktopAccessor.dll")]
        static extern int GetCurrentDesktopNumber();

        private int checkDesktop()
        {
            Debug.WriteLine("Current Desktop: " + GetCurrentDesktopNumber());
            return GetCurrentDesktopNumber();
        }

        public void ResetTimer()
        {
            elapsedTime = TimeSpan.Zero;
            if (Current.MainWindow is MainWindow mainWindow && mainWindow.IsLoaded)
            {
                mainWindow.UpdateTimeDisplay(elapsedTime);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            timer?.Stop();
            notifyIcon?.Dispose();
            base.OnExit(e);
        }

        private void OpenApp(object sender, RoutedEventArgs e)
        {
            MainWindow? window = Current.MainWindow as MainWindow;
            if (window == null)
            {
                window = new MainWindow();
                Current.MainWindow = window;
            }
            
            window.Show();
            window.WindowState = WindowState.Normal;
            window.Activate();
            
            // Update display with current time
            window.UpdateTimeDisplay(elapsedTime);
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }
    }
}
