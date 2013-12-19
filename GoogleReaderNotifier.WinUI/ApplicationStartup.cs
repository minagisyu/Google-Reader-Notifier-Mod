using System;
using System.Windows.Forms;

namespace GoogleReaderNotifier.WinUI
{
	/// <summary>
	/// Summary description for Main.
	/// </summary>
	public class ApplicationStartup
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}
	}
}
