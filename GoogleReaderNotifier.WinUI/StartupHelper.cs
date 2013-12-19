using System;
using Microsoft.Win32;
using System.Windows.Forms;

namespace GoogleReaderNotifier.WinUI
{
	/// <summary>
	/// Summary description for StartupHelper.
	/// </summary>
	public class StartupHelper
	{
		public static void StartWithWindows(bool shouldStartWithWindows)
		{
			// The path to the key where Windows looks for startup applications
			RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

			if (shouldStartWithWindows)
			{
				// Add the value in the registry so that the application runs at startup
				rkApp.SetValue("GoogleRdrNotify", "\"" + Application.ExecutablePath.ToString() + "\"");
			}
			else
			{
				// Remove the value from the registry so that the application doesn't start
				rkApp.DeleteValue("GoogleRdrNotify", false);
			}
		}
		public static bool AppStartsWithWindows()
		{
			// The path to the key where Windows looks for startup applications
			RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

			// Check to see the current state (running at startup or not)
			if (rkApp.GetValue("GoogleRdrNotify") == null)
			{
				// The value doesn't exist, the application is not set to run at startup
				return false;
			}
			else
			{
				// The value exists, the application is set to run at startup
				return true;
			}
		}
	}
}
