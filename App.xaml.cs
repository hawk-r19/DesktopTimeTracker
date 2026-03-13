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
        private TrayPopup? trayPopup;
        private DispatcherTimer? uiUpdateTimer;
        private Stopwatch stopwatch = new Stopwatch();
        private bool[] targetDesktops = new bool[5]; // Support up to 5 desktops
        private int currentDesktop;
        private HwndSource? hwndSource;
        private readonly object timerLock = new object();
        private bool timerPaused = false;

        // Custom message ID for desktop changes
        private const uint DESKTOP_CHANGE_MESSAGE_OFFSET = 0x0139;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // default desktop to track
            targetDesktops[3] = true; // Track desktop 4 (0-based index)

            // Initialize the system tray icon
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            
            // Get reference to the TrayToolTip
            trayPopup = notifyIcon.TrayPopup as TrayPopup;

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

        public int getDesktopCount()
        {
            return VirtualDesktop.GetDesktopCount();
        }

        public bool isTargetDesktop(int desktop)
        {
            if (desktop < 0 || desktop >= targetDesktops.Length)
                return false;
            return targetDesktops[desktop];
        }

        public void toggleDesktopTracking(int desktop)
        {
            if (desktop < 0 || desktop >= targetDesktops.Length)
                return;
            targetDesktops[desktop] = !targetDesktops[desktop];
            UpdateTimerState();
        }

        public bool[] getActiveDesktops()
        {
            return targetDesktops;
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

                if (isTargetDesktop(currentDesktop) && !timerPaused)
                {
                    // Resume timer when on target desktop and not paused by user
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
                        Debug.WriteLine("Timer paused");
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
                if (isTargetDesktop(currentDesktop))
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
            TimeSpan elapsed;
            bool isRunning;
            
            lock (timerLock)
            {
                if (stopwatch == null)
                    return;
                
                elapsed = stopwatch.Elapsed;
                isRunning = stopwatch.IsRunning;
            }
            
            // Update main window if loaded
            if (Current.MainWindow is MainWindow mainWindow && mainWindow.IsLoaded)
            {
                mainWindow.UpdateTimeDisplay(elapsed, isRunning, timerPaused);
            }
            
            // Update tray popup
            trayPopup?.UpdateTimer(elapsed, isRunning, timerPaused);
        }

        public void ResetTimer()
        {
            bool isRunning = true;
            lock (timerLock)
            {
                if (stopwatch == null)
                {
                    stopwatch = new Stopwatch();
                }
                
                stopwatch.Restart();
                
                // Pause immediately if not on target desktop
                if (isTargetDesktop(currentDesktop) || timerPaused)
                {
                    stopwatch.Stop();
                    isRunning = false;
                }
            }
            
            if (Current.MainWindow is MainWindow mainWindow && mainWindow.IsLoaded)
            {
                mainWindow.UpdateTimeDisplay(TimeSpan.Zero, isRunning, timerPaused);
            }
        }

        public void TogglePauseTimer()
        {
            timerPaused = !timerPaused;
            UpdateTimerState();
        }

        public bool isTimerPaused()
        {
            return timerPaused;
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

        public void OpenMainWindow()
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
            window.UpdateTimeDisplay(elapsed, stopwatch.IsRunning, timerPaused);
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

        private void TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            OpenMainWindow();
        }

        private void OpenApp(object sender, RoutedEventArgs e)
        {
            OpenMainWindow();
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }
    }
}
