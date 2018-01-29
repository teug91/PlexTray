﻿using System;
using System.Windows;

namespace PlexTray.GUI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : ToolWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();

            if (SettingsManager.GetHost() != null)
                HostTextBox.Text = SettingsManager.GetHost().ToString();

            if (SettingsManager.GetPlexToken() != null)
                PlexTokenTextBox.Text = SettingsManager.GetPlexToken();

            if (SettingsManager.GetPushbulletToken() != null)
                PushbulletTokenTextBox.Text = SettingsManager.GetPushbulletToken();

            InitializeAutoStartCheckbox();
        }

        /// <summary>
        /// Sets checkbox to value from settings.
        /// </summary>
        private void InitializeAutoStartCheckbox()
        {
            bool? autoStart = SettingsManager.GetAutoStart();

            if (autoStart == null)
                autostartCheckbox.Visibility = Visibility.Hidden;
            else
                autostartCheckbox.IsChecked = autoStart;
        }

        /// <summary>
        /// Occurs when 'Save' button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool? autoStart = null;

            if (autostartCheckbox.Visibility == Visibility.Visible)
                autoStart = autostartCheckbox.IsChecked;

            Uri newHost;
            if (!Uri.TryCreate(HostTextBox.Text, UriKind.Absolute, out newHost))
                InvalidHost.Visibility = Visibility.Visible;

            else
            {
                SettingsManager.SaveSettings(newHost, PlexTokenTextBox.Text, PushbulletTokenTextBox.Text, autoStart);
                Close();
            }
        }

        /// <summary>
        /// Occurs when 'Cancel' button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
