# DesktopTimeTracker

A very basic timer to track time spent in specific virtual desktops in Windows.

Run the app once and leave it running in the background, and it will automatically detect whenever you are currently on the target desktop you want to track.

I made this to track billable hours on a project I'm working on myself for a client. I kept forgetting to turn the timer on and off, so I made this app to automatically do so. This method of tracking virtual desktops is a workaround for tracking specific applications. It also lets you take frequent breaks without having to worry about timing at all.

## Installation
1. Download a release
2. Extract to a folder of your choice
3. Run `DesktopTimeTracker.exe`

## Usage and Features

Upon launch, tracks desktop 4 by default. Starts counting time immediately if on the selected desktop. This is when not on desktop 4.
![img dtt1](https://github.com/hawk-r19/DesktopTimeTracker/blob/main/imgs/dtt1.png)    

After switching to the active desktop, time starts being tracked.  
![img dtt2](https://github.com/hawk-r19/DesktopTimeTracker/blob/main/imgs/dtt2.png)  

Can manually pause tracking, requires manually resuming to activate automatic desktop detection. 
![img dtt3](https://github.com/hawk-r19/DesktopTimeTracker/blob/main/imgs/dtt3.png)   

Can select multiple virtual desktops to contribute to the tracked time.  
![img dtt4](https://github.com/hawk-r19/DesktopTimeTracker/blob/main/imgs/dtt4.png)  

System tray icon popup from single-click for quick access to time and controls.  
![img dtt5](https://github.com/hawk-r19/DesktopTimeTracker/blob/main/imgs/dtt5.png)  

## Future Plans

Depending on how much I use this app, I may add more features to it that other popular time trackers on the market have, such as a more extensive UI with a calendar to track time, seperate projects to allot time to, etc. I mainly made this for personal use, so I don't have any concrete plans.

## Dependencies

- [Ciantic's VirtualDesktopAccessor](https://github.com/Ciantic/VirtualDesktopAccessor) under MIT License.  
- Hardcodet.NotifyIcon.Wpf v2.0.1
