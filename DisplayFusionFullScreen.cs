using System;
using System.Drawing;

// The 'windowHandle' parameter will contain the window handle for the:
//   - Active window when run by hotkey
//   - Window Location target when run by a Window Location rule
//   - TitleBar Button owner when run by a TitleBar Button
//   - Jump List owner when run from a Taskbar Jump List
//   - Currently focused window if none of these match
public static class DisplayFusionFunction
{
	public static void Run(IntPtr windowHandle)
	{
		//Hide Taskbar
		BFS.Taskbar.SetWindowsTaskbarAutoHide(true);
		
		//get the window styles
		BFS.WindowEnum.WindowStyle style = BFS.Window.GetWindowStyle(windowHandle);
		
		//if the window has any borders and titles, make it full screen
		if(BFS.Window.HasWindowStyle(BFS.WindowEnum.WindowStyle.WS_CAPTION, windowHandle) ||
			BFS.Window.HasWindowStyle(BFS.WindowEnum.WindowStyle.WS_SYSMENU, windowHandle) ||
			BFS.Window.HasWindowStyle(BFS.WindowEnum.WindowStyle.WS_THICKFRAME__SIZEBOX, windowHandle) ||
			BFS.Window.HasWindowStyle(BFS.WindowEnum.WindowStyle.WS_MINIMIZEBOX, windowHandle))
			 //|| BFS.Window.HasWindowStyle(BFS.WindowEnum.WindowStyle.WS_MAXIMIZEBOX, windowHandle))
		{
			//save the size and position of the window
			SaveWindowSize(windowHandle);
			
			//make sure to remove the styles. just toggling these settings may turn on something we dont want
			style &= ~BFS.WindowEnum.WindowStyle.WS_CAPTION;
			style &= ~BFS.WindowEnum.WindowStyle.WS_SYSMENU;
			style &= ~BFS.WindowEnum.WindowStyle.WS_THICKFRAME__SIZEBOX;
			style &= ~BFS.WindowEnum.WindowStyle.WS_MINIMIZEBOX;
			//style &= ~BFS.WindowEnum.WindowStyle.WS_MAXIMIZEBOX;
		
			//get the bounds of the monitor that the window is in
			Rectangle bounds = BFS.Monitor.GetMonitorBoundsByWindow(windowHandle);
			
			//set the window style
			BFS.Window.SetWindowStyle(style, windowHandle);
						
			//size and position the window to be fullscreen within the monitor
			BFS.Window.SetSizeAndLocation(windowHandle, bounds.X, bounds.Y, bounds.Width, bounds.Height);
		}
		else
		{
			//if we got here, then the window must already be fullscreen.
			//add non-fullscreen styles back to the window
			style |= BFS.WindowEnum.WindowStyle.WS_CAPTION;
			style |= BFS.WindowEnum.WindowStyle.WS_SYSMENU;
			style |= BFS.WindowEnum.WindowStyle.WS_THICKFRAME__SIZEBOX;
			style |= BFS.WindowEnum.WindowStyle.WS_MINIMIZEBOX;
			style |= BFS.WindowEnum.WindowStyle.WS_MAXIMIZEBOX;
			
			//try and load saved window size and position
			bool isMaximized;
			Rectangle bounds = GetSavedWindowSize(windowHandle, out isMaximized);
			
			//set the window style
			BFS.Window.SetWindowStyle(style, windowHandle);
			
			//if we couldnt load the size, exit the script
			if(bounds.Equals(Rectangle.Empty))
				return;
				
			//if the window was maximized, maximize it, otherwise set the window size and location with the values we loaded
			if(isMaximized)
				BFS.Window.Maximize(windowHandle);
			else
				BFS.Window.SetSizeAndLocation(windowHandle, bounds.X, bounds.Y, bounds.Width, bounds.Height);
		}
		
		WaitExitAndClose(windowHandle);
	}
	
	//this is a function that will save the window size and position in window properties
	private static void SaveWindowSize(IntPtr windowHandle)
	{
		Rectangle bounds = BFS.Window.GetBounds(windowHandle);
		BFS.Window.SetWindowProperty(windowHandle, "ToggleFullscreen_X", new IntPtr(bounds.X));
		BFS.Window.SetWindowProperty(windowHandle, "ToggleFullscreen_Y", new IntPtr(bounds.Y));
		BFS.Window.SetWindowProperty(windowHandle, "ToggleFullscreen_Width", new IntPtr(bounds.Width));
		BFS.Window.SetWindowProperty(windowHandle, "ToggleFullscreen_Height", new IntPtr(bounds.Height));
		BFS.Window.SetWindowProperty(windowHandle, "ToggleFullscreen_IsMaximized", new IntPtr(BFS.Window.IsMaximized(windowHandle) ? 1 : 0));
	}
	
	private static Rectangle GetSavedWindowSize(IntPtr windowHandle, out bool isMaximized)
	{
		Rectangle bounds = new Rectangle();
		bounds.X = BFS.Window.GetWindowProperty(windowHandle, "ToggleFullscreen_X").ToInt32();
		bounds.Y = BFS.Window.GetWindowProperty(windowHandle, "ToggleFullscreen_Y").ToInt32();
		bounds.Width = BFS.Window.GetWindowProperty(windowHandle, "ToggleFullscreen_Width").ToInt32();
		bounds.Height = BFS.Window.GetWindowProperty(windowHandle, "ToggleFullscreen_Height").ToInt32();
		isMaximized = (BFS.Window.GetWindowProperty(windowHandle, "ToggleFullscreen_IsMaximized").ToInt32() == 1);
		return bounds;
	}
	
	private static void WaitExitAndClose(IntPtr windowHandle){
		uint appID = BFS.Application.GetAppIDByWindow(windowHandle);
		BFS.Application.WaitForExitByAppID(appID, 0);
		BFS.Taskbar.SetWindowsTaskbarAutoHide(false);
		BFS.Window.Close(windowHandle);
	}
}