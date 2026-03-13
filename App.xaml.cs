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
        private readonly object timerLock = new object();
        
        // Custom message ID for desktop changes
        private const uint DESKTOP_CHANGE_MESSAGE_OFFSET = 0x0139;

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
            // Create HwndSource for receiving Windows messages
            HwndSourceParameters parameters = new HwndSourceParameters("VirtualDesktopListener")
            {
                Width = 0,
                Height = 0,
                PositionX = 0,
                PositionY = 0,
                WindowStyle = 0
            };

            hwndSource = new HwndSource(parameters);
            hwndSource.AddHook(WndProc);
            
            // Register for desktop change messages
            VirtualDesktop.RegisterPostMessageHook(hwndSource.Handle, DESKTOP_CHANGE_MESSAGE_OFFSET);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Log ALL messages for debugging and finding correct offset (causes crashes)
            //Debug.WriteLine($"Message: 0x{msg:X4}, wParam={wParam.ToInt32()}, lParam={lParam.ToInt32()}");

            if (msg == DESKTOP_CHANGE_MESSAGE_OFFSET)
            {
                int oldDesktop = wParam.ToInt32();
                int newDesktop = lParam.ToInt32();
                
                Debug.WriteLine($"Desktop changed: {oldDesktop} -> {newDesktop}");
                OnDesktopChanged(oldDesktop, newDesktop);
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void OnDesktopChanged(int oldDesktop, int newDesktop)
        {
            // Check if we're already on the UI thread
            if (Dispatcher.CheckAccess())
            {
                currentDesktop = newDesktop;
                UpdateTimerState();
            }
            else
            {
                // Use BeginInvoke instead of Invoke to avoid blocking
                Dispatcher.BeginInvoke(() =>
                {
                    currentDesktop = newDesktop;
                    UpdateTimerState();
                });
            }
        }

        private void UpdateTimerState()
        {
            lock (timerLock)
            {
                if (stopwatch == null)
                    return;

                if (currentDesktop == targetDesktop)
                {
                    // Resume timer when on target desktop
                    if (!stopwatch.IsRunning)
                    {
                        stopwatch.Start();
                        Debug.WriteLine("Timer resumed - on target desktop");
                    }
                }
                else
                {
                    // Pause timer when not on target desktop
                    if (stopwatch.IsRunning)
                    {
                        stopwatch.Stop();
                        Debug.WriteLine("Timer paused - not on target desktop");
                    }
                }
            }
        }

        private void InitializeTimer()
        {
            lock (timerLock)
            {
                // Use Stopwatch for accurate time tracking
                stopwatch = new Stopwatch();
                
                // Start only if on target desktop
                if (currentDesktop == targetDesktop)
                {
                    stopwatch.Start();
                }
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
                TimeSpan elapsed;
                lock (timerLock)
                {
                    if (stopwatch == null)
                        return;
                    
                    elapsed = stopwatch.Elapsed;
                }
                
                mainWindow.UpdateTimeDisplay(elapsed);
            }
        }

        public void ResetTimer()
        {
            lock (timerLock)
            {
                if (stopwatch == null)
                {
                    stopwatch = new Stopwatch();
                }
                
                stopwatch.Restart();
                
                // Pause immediately if not on target desktop
                if (currentDesktop != targetDesktop)
                {
                    stopwatch.Stop();
                }
            }
            
            if (Current.MainWindow is MainWindow mainWindow && mainWindow.IsLoaded)
            {
                mainWindow.UpdateTimeDisplay(TimeSpan.Zero);
            }
        }

        public void PauseTimer()
        {
            lock (timerLock)
            {
                stopwatch?.Stop();
            }
        }

        public void ResumeTimer()
        {
            lock (timerLock)
            {
                stopwatch?.Start();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            uiUpdateTimer?.Stop();
            lock (timerLock)
            {
                stopwatch?.Stop();
            }
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
            TimeSpan elapsed;
            lock (timerLock)
            {
                elapsed = stopwatch?.Elapsed ?? TimeSpan.Zero;
            }
            window.UpdateTimeDisplay(elapsed);
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }
    }
}
