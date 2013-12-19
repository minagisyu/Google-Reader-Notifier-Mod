using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Resources;
using System.Windows.Forms;

using GoogleReaderNotifier.ReaderAPI;

namespace GoogleReaderNotifier.WinUI
{
	/// <summary>
	/// Summary description for MainForm.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.NotifyIcon _notifyIcon;
		private System.Windows.Forms.ContextMenu _contextMenu;
		private System.Windows.Forms.Timer _timer;
		private System.Windows.Forms.MenuItem _goToReaderMenuItem;
		private System.Windows.Forms.MenuItem _checkNowMenuItem;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem _preferencesMenuItem;
		private System.Windows.Forms.MenuItem _exitMenuItem;
		private System.ComponentModel.IContainer components;

		private bool _showCountTooltip = false;
		private string _labelFilter= string.Empty;
		private bool _animatePopup = true;
		private string _username = string.Empty;
		private string _password = string.Empty;

		private int _currentUnreadCount = 0;
		private TaskbarNotifier _trayNotifier;
		

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			InitializeTrayNotifier();
		}

		#region Dispose()

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this._notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this._contextMenu = new System.Windows.Forms.ContextMenu();
			this._goToReaderMenuItem = new System.Windows.Forms.MenuItem();
			this._checkNowMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this._preferencesMenuItem = new System.Windows.Forms.MenuItem();
			this._exitMenuItem = new System.Windows.Forms.MenuItem();
			this._timer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// _notifyIcon
			// 
			this._notifyIcon.ContextMenu = this._contextMenu;
			this._notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("_notifyIcon.Icon")));
			this._notifyIcon.Text = "Google Reader Notifier";
			this._notifyIcon.Visible = true;
			this._notifyIcon.DoubleClick += new System.EventHandler(this.HandleNotifyIconDoubleClicked);
			// 
			// _contextMenu
			// 
			this._contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._goToReaderMenuItem,
            this._checkNowMenuItem,
            this.menuItem3,
            this._preferencesMenuItem,
            this._exitMenuItem});
			// 
			// _goToReaderMenuItem
			// 
			this._goToReaderMenuItem.DefaultItem = true;
			this._goToReaderMenuItem.Index = 0;
			this._goToReaderMenuItem.Text = "Go to Reader...";
			this._goToReaderMenuItem.Click += new System.EventHandler(this.HandleGoToReaderMenuItemClicked);
			// 
			// _checkNowMenuItem
			// 
			this._checkNowMenuItem.Index = 1;
			this._checkNowMenuItem.Text = "Check Now";
			this._checkNowMenuItem.Click += new System.EventHandler(this.HandleCheckNowMenuItemClicked);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 2;
			this.menuItem3.Text = "-";
			// 
			// _preferencesMenuItem
			// 
			this._preferencesMenuItem.Index = 3;
			this._preferencesMenuItem.Text = "Preferences...";
			this._preferencesMenuItem.Click += new System.EventHandler(this.HandlePreferencesMenuItemClicked);
			// 
			// _exitMenuItem
			// 
			this._exitMenuItem.Index = 4;
			this._exitMenuItem.Text = "Exit";
			this._exitMenuItem.Click += new System.EventHandler(this.HandleExitMenuItemClicked);
			// 
			// _timer
			// 
			this._timer.Tick += new System.EventHandler(this._timer_Tick);
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.ClientSize = new System.Drawing.Size(248, 120);
			this.ControlBox = false;
			this.Enabled = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.Opacity = 0;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Menu Click Handlers

		private void HandleGoToReaderMenuItemClicked(object sender, System.EventArgs e)
		{
			this.GoToReader();
		}

		private void HandleCheckNowMenuItemClicked(object sender, System.EventArgs e)
		{
			this.CheckForUpdates();
		}

		private void HandlePreferencesMenuItemClicked(object sender, System.EventArgs e)
		{
			this.DisplayPreferences();
		}

		private void HandleExitMenuItemClicked(object sender, System.EventArgs e)
		{
			this.ExitApplication();
		}

		#endregion

 		private void MainForm_Load(object sender, System.EventArgs e)
		{
			this.LoadSettings();
		}       

		#region TrayNotification 

		private void InitializeTrayNotifier()
		{
			_trayNotifier = new TaskbarNotifier();
			_trayNotifier.SetBackgroundBitmap(new Bitmap(GetType(),"Images.skin.bmp"),Color.FromArgb(255,0,255));
			_trayNotifier.SetCloseBitmap(new Bitmap(GetType(),"Images.close.bmp"),Color.FromArgb(255,0,255),new Point(243,7));
			_trayNotifier.TitleRectangle=new Rectangle(40,9,70,25);
			_trayNotifier.ContentRectangle=new Rectangle(78,15,150,20);
			_trayNotifier.NormalContentFont = new Font("Arial",12,FontStyle.Bold,GraphicsUnit.Pixel);
			_trayNotifier.HoverContentFont = new Font("Arial",12,FontStyle.Bold,GraphicsUnit.Pixel);
			_trayNotifier.HoverContentColor = Color.FromArgb(123,150,198);
			_trayNotifier.KeepVisibleOnMousOver=true;
			_trayNotifier.TopLevel = true;

			_trayNotifier.ContentClick += new EventHandler(HandleTrayNotifierContentClicked);
		}


		private void HandleTrayNotifierContentClicked(object sender, System.EventArgs e)
		{
			System.Diagnostics.Process.Start(ConfigurationSettings.AppSettings["GoogleReaderUrl"]);
			ResetTrayIcon();
		}

		private void HandleNotifyIconDoubleClicked(object sender, System.EventArgs e)
		{
			this.GoToReader();
		}

		private void ShowTrayNotification(string displayText)
		{
			_trayNotifier.Visible = true;
			_trayNotifier.Show("",displayText,300,5000,500);
		}

		private void ResetTrayIcon()
		{
			ResourceManager resources = new ResourceManager(typeof(MainForm));
			_notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("_notifyIcon.Icon")));
		}

		#endregion

		#region Private methods

		private void CheckForUpdates()
		{
			try
			{
				//verify the user has entered credentials
				if (!this.VerifyLoginEntered())
					return;

				//call the Google API
				GoogleReader reader = new GoogleReader();
				string result = reader.GetDetailedCount(_username, _password, _labelFilter);
			
				// if there was an error, disable the timer for now.
				if(result == "AUTH_ERROR")
				{
				//	_timer.Stop();
				}
				else
				{
					_timer.Start();
					this.DisplayResults(reader);
				}
			}
			catch{}
		}

		private bool VerifyLoginEntered()
		{
			DialogResult result = DialogResult.None;

			//if not user/password, prompt user
			if ((_username.Trim().Length == 0) || (_password.Trim().Length == 0))
			{
				result = this.DisplayPreferences();
				return (result == DialogResult.OK);
			}
			else
			{
				return true;
			}
			
		}

		private void DisplayResults(GoogleReader reader)
		{
			if(reader.TotalCount > 0)
			{
				string result = reader.TotalCount.ToString() + " unread items";

				// if the option is enabled, change the tooltip text
				if(_showCountTooltip)
				{
					if(result.Length < 64)
					{
						_notifyIcon.Text = result;
					}
				}
				if(_currentUnreadCount < reader.TotalCount)
				{
					// if animation is turned on, show the animation code
					if(_animatePopup)
					{
						this.ShowTrayNotification(result);
					}
				}
				_notifyIcon.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("GoogleReaderNotifier.WinUI.Images.unread.ico"));
				_currentUnreadCount = reader.TotalCount;
			}
			else
			{
				_currentUnreadCount = 0;
				ResetTrayIcon();
				_notifyIcon.Text = "Google Reader Notifier";
			}
		}

		

		private DialogResult DisplayPreferences()
		{
			PreferencesForm frm = new PreferencesForm();
			DialogResult result = frm.ShowDialog();
			if(result == DialogResult.OK)
			{
				LoadSettings();
				CheckForUpdates();
			}
			return result;
		}

		private void ExitApplication()
		{
			//reset the "start with windows" setting
			UserPreferences prefs = PreferencesHelper.RetrievePreferences();
			StartupHelper.StartWithWindows(prefs.StartAtWindowsStartup);

			_notifyIcon.Visible = false;
			Application.Exit();
		}

		private void GoToReader()
		{
			System.Diagnostics.Process.Start(ConfigurationSettings.AppSettings["GoogleReaderUrl"]);
			ResetTrayIcon();
		}

	

		private void LoadSettings()
		{
			_timer.Stop();

			UserPreferences prefs = PreferencesHelper.RetrievePreferences();
			_timer.Interval = prefs.TimerMinutes * 60000;
			_showCountTooltip = prefs.ShowCountTooltip;
			_animatePopup = prefs.AnimatePopup;
			_labelFilter = prefs.FilterLabels;
			_username = prefs.Username;
			_password = prefs.Password;

			StartupHelper.StartWithWindows(prefs.StartAtWindowsStartup);

			if (!_showCountTooltip)
			{
				_notifyIcon.Text = ConfigurationSettings.AppSettings["NotifyIconTitle"];
			}
			
			if ((_username.Trim().Length == 0) || (_password.Trim().Length == 0))
			{
				this.DisplayPreferences();
			}
			_timer.Start();

			CheckForUpdates();

		}


		#endregion

		private void _timer_Tick(object sender, EventArgs e)
		{
			CheckForUpdates();
		}

	}
}
