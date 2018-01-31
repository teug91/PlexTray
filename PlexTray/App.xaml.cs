using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PlexTray
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        private string name;

        /// <summary>
        /// Creates tray icon and starts listening for input.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Plex.Init();
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            notifyIcon.TrayBalloonTipClicked += TrayBalloonTipClicked;
			
            Pushbullet.Communicator.Init();
            Pushbullet.Communicator.PushRecieved += ShowBalloon;
			//ShowBalloon("Blue Velvet (1986)", null);
			//ShowBalloon("One Tree Hill - S04E02 - Things I Forgot at Birth", null);
		}

        private void TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            Plex.OpenPlexMedia(name);
        }

        private void ShowBalloon(object sender, EventArgs e)
        {
            name = (string) sender;
            notifyIcon.ShowBalloonTip("File added", name, BalloonIcon.None);
        }

        /// <summary>
        /// Makes sure tray icon is removed when application exits.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnExit(e);
        }

        /// <summary>
        /// New settings, reinitializes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingSaving(object sender, CancelEventArgs e)
        {
            Plex.Init();
        }
    }
}