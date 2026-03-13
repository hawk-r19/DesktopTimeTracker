using Hardcodet.Wpf.TaskbarNotification;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace DesktopTimeTracker
{
    public partial class App : Application
    {
        private TaskbarIcon? notifyIcon;
        private DispatcherTimer? uiUpdateTimer;
        private Stopwatch? stopwatch;
        private int targetDesktop = 3;
        private int currentDesktop;
        private HwndSource? hwndSource;
        
        // Custom message ID for desktop changes
        // The message will be WM_USER + messageOffset that we specify
        private const int WM_USER = 0x0400;
        private const uint DESKTOP_CHANGE_MESSAGE_OFFSET = 0x0139; // Use a specific offset
        //private const int WM_DESKTOP_CHANGED = WM_USER + (int)DESKTOP_CHANGE_MESSAGE_OFFSET;
        private const int WM_DESKTOP_CHANGED = 0x0139;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize the system tray icon
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            // Create a hidden window to receive desktop change messages
            CreateMessageWindow();

            // Initialize current desktop and timer
            currentDesktop = VirtualDesktop.GetCurrentDesktopNumber();
            Debug.WriteLine($"Current desktop on startup: {currentDesktop}");
            InitializeTimer();
            
            // Start or pause based on initial desktop
            UpdateTimerState();
        }

        private void CreateMessageWindow()
        {
            // Create HwndSource directly instead of using a WPF Window
            HwndSourceParameters parameters = new HwndSourceParameters("VirtualDesktopListener")
            {
                Width = 0,
                Height = 0,
                PositionX = 0,
                PositionY = 0,
                WindowStyle = 0
            };

            hwndSource = new HwndSource(parameters);
            
            // Add the hook to process messages
            hwndSource.AddHook(WndProc);
            
            // Get the window handle
            IntPtr hwnd = hwndSource.Handle;
            Debug.WriteLine($"Message window handle: {hwnd:X}");

            // Register for desktop change messages
            int result = VirtualDesktop.RegisterPostMessageHook(hwnd, DESKTOP_CHANGE_MESSAGE_OFFSET);
            Debug.WriteLine($"RegisterPostMessageHook result: {result}");
            Debug.WriteLine($"Listening for messages at: 0x{WM_DESKTOP_CHANGED:X} (WM_USER+{DESKTOP_CHANGE_MESSAGE_OFFSET})");
            
            if (result == 0)
            {
                Debug.WriteLine("WARNING: RegisterPostMessageHook returned 0 - registration may have failed");
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Log ALL messages for debugging (comment out after you find the right one)
            Debug.WriteLine($"Message: 0x{msg:X4}, wParam={wParam.ToInt32()}, lParam={lParam.ToInt32()}");

            if (msg == WM_DESKTOP_CHANGED)
            {
                // The desktop numbers might be in different parameters
                // Try both wParam and lParam
                int newDesktop = wParam.ToInt32();
                int oldDesktop = lParam.ToInt32();
                
                Debug.WriteLine($"*** DESKTOP CHANGE DETECTED: {oldDesktop} -> {newDesktop} ***");
                OnDesktopChanged(oldDesktop, newDesktop);
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void OnDesktopChanged(int oldDesktop, int newDesktop)
        {
            Debug.WriteLine($"Desktop changed from {oldDesktop} to {newDesktop}");
            
            // Update on UI thread
            Dispatcher.Invoke(() =>
            {
                currentDesktop = newDesktop;
                UpdateTimerState();
            });
        }

        private void UpdateTimerState()
        {
            if (currentDesktop == targetDesktop)
            {
                // Resume timer when on target desktop
                if (stopwatch?.IsRunning == false)
                {
                    stopwatch?.Start();
                    Debug.WriteLine("Timer resumed - on target desktop");
                }
            }
            else
            {
                // Pause timer when not on target desktop
                if (stopwatch?.IsRunning == true)
                {
                    stopwatch?.Stop();
                    Debug.WriteLine("Timer paused - not on target desktop");
                }
            }
        }

        private void InitializeTimer()
        {
            // Use Stopwatch for accurate time tracking
            stopwatch = new Stopwatch();
            
            // Start only if on target desktop
            if (currentDesktop == targetDesktop)
            {
                stopwatch.Start();
            }

            // Use DispatcherTimer only for UI updates
            uiUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            uiUpdateTimer.Tick += UiUpdateTimer_Tick;
            uiUpdateTimer.Start();
        }

        private void UiUpdateTimer_Tick(object? sender, EventArgs e)
        {
            // Update UI with current elapsed time
            if (Current.MainWindow is MainWindow mainWindow && mainWindow.IsLoaded)
            {
                var elapsed = stopwatch?.Elapsed ?? TimeSpan.Zero;
                mainWindow.UpdateTimeDisplay(elapsed);
            }
        }

        public void ResetTimer()
        {
            stopwatch?.Restart();
            
            // Pause immediately if not on target desktop
            if (currentDesktop != targetDesktop)
            {
                stopwatch?.Stop();
            }
            
            if (Current.MainWindow is MainWindow mainWindow && mainWindow.IsLoaded)
            {
                mainWindow.UpdateTimeDisplay(TimeSpan.Zero);
            }
        }

        public void PauseTimer()
        {
            stopwatch?.Stop();
        }

        public void ResumeTimer()
        {
            stopwatch?.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            uiUpdateTimer?.Stop();
            stopwatch?.Stop();
            hwndSource?.RemoveHook(WndProc);
            hwndSource?.Dispose();
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
            window.UpdateTimeDisplay(stopwatch?.Elapsed ?? TimeSpan.Zero);
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }
    }
}
