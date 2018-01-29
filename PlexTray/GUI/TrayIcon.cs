using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace PlexTray.GUI
{
    internal class TrayIcon : INotifyPropertyChanged
    {
        string icon = "/PlexTray;component/Resources/Plex.ico";

        public TrayIcon()
        {
            Timer initializationTimer = new Timer(3000);
            initializationTimer.Elapsed += SetIcon;
            initializationTimer.Enabled = true;
            GC.KeepAlive(initializationTimer);
        }

        /// <summary>
        /// Sets icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SetIcon(object sender, ElapsedEventArgs e)
        {
            bool isAvailable = Plex.IsAvailable();

            if (isAvailable)
            {
                if (Icon != "/PlexTray;component/Resources/Plex.ico")
                {
                    Icon = "/PlexTray;component/Resources/Plex.ico";
                    OnPropertyChanged("Icon");
                }
            }

            else
            {
                if (Icon != "/PlexTray;component/Resources/error.ico")
                {
                    Icon = "/PlexTray;component/Resources/error.ico";
                    OnPropertyChanged("Icon");
                }
            }
        }

        /// <summary>
        ///     Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        if (Application.Current.MainWindow != null)
                        {
                            Application.Current.MainWindow.Activate();
                        }

                        else
                        {
                            Application.Current.MainWindow = new SettingsWindow();
                            Application.Current.MainWindow.Show();
                            Application.Current.MainWindow.Activate();
                        }
                    }
                };
            }
        }

        /// <summary>
        ///     Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get { return new DelegateCommand {CommandAction = () => Application.Current.Shutdown()}; }
        }

        /// <summary>
        ///     Pause/resume all torrents.
        /// </summary>
        public ICommand UpdateLibraries
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        Plex.UpdateLibraries();
                    }
                };
            }
        }

        /// <summary>
        /// Open qBittorrent WebUI in standard browser.
        /// </summary>
        public ICommand OpenWebUi
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        Plex.OpenPlex();
                    }
                };
            }
        }

        /// <summary>
        /// Gets icon.
        /// </summary>
        public string Icon
        {
            get
            {
                return icon;
            }

            set
            {
                icon = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    ///     Simplistic delegate command for the demo.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}