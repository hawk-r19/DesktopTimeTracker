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
        private DispatcherTimer? uiUpdateTimer;
        private Stopwatch? stopwatch;
        private int desktop = 3;
        private bool paused = false;

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
            stopwatch = new Stopwatch();
            stopwatch.Start();

            uiUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // Update UI more frequently for smoothness
            };
            uiUpdateTimer.Tick += UiUpdateTimer_Tick;
            uiUpdateTimer.Start();
        }

        private void UiUpdateTimer_Tick(object? sender, EventArgs e)
        {
            // Pause/resume based on desktop
            if (checkDesktop() != desktop)
            {
                if (stopwatch?.IsRunning == true)
                    stopwatch?.Stop();
            }
            else
            {
                if (stopwatch?.IsRunning == false)
                    stopwatch?.Start();
            }

            // Update UI
            if (Current.MainWindow is MainWindow mainWindow && mainWindow.IsLoaded)
            {
                var elapsed = stopwatch?.Elapsed ?? TimeSpan.Zero;
                mainWindow.UpdateTimeDisplay(elapsed);
            }
        }

        [DllImport("VirtualDesktopAccessor.dll")]
        static extern int GetCurrentDesktopNumber();

        private int checkDesktop()
        {
            Debug.WriteLine("Current Desktop: " + GetCurrentDesktopNumber());
            return GetCurrentDesktopNumber();
        }

        public TimeSpan GetElapsedTime()
        {
            return stopwatch?.Elapsed ?? TimeSpan.Zero;
        }

        public void ResetTimer()
        {
            stopwatch?.Restart();
            if (Current.MainWindow is MainWindow mainWindow && mainWindow.IsLoaded)
            {
                mainWindow.UpdateTimeDisplay(TimeSpan.Zero);
            }
        }

        public void PauseTimer()
        {
            paused = true;
            stopwatch?.Stop();
        }

        public void ResumeTimer()
        {
            paused = false;
            stopwatch?.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            uiUpdateTimer?.Stop();
            stopwatch?.Stop();
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
            window.UpdateTimeDisplay(GetElapsedTime());
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }
    }
}
