using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Caliburn.Micro;
using loadify.Audio;
using loadify.Configuration;
using loadify.Event;

namespace loadify.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        public bool UseProxy
        {
            get { return _SettingsManager.ConnectionSetting.UseProxy; }
            set
            {
                if (_SettingsManager.ConnectionSetting.UseProxy == value) return;
                _SettingsManager.ConnectionSetting.UseProxy = value;
                NotifyOfPropertyChange(() => UseProxy);
            }
        }

        public string ProxyIp
        {
            get { return _SettingsManager.ConnectionSetting.ProxyIp; }
            set
            {
                if (_SettingsManager.ConnectionSetting.ProxyIp == value) return;

                if (!Regex.IsMatch(ProxyIp,
                    @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$"))
                {
                    _EventAggregator.PublishOnUIThread(new NotificationEvent("Settings Error", 
                            "The proxy IP address that was entered is not a valid IP address."));
                }

                _SettingsManager.ConnectionSetting.ProxyIp = value;
                NotifyOfPropertyChange(() => ProxyIp);
            }
        }

        public ushort ProxyPort
        {
            get { return _SettingsManager.ConnectionSetting.ProxyPort; }
            set
            {
                if (_SettingsManager.ConnectionSetting.ProxyPort == value) return;
                _SettingsManager.ConnectionSetting.ProxyPort = value;
                NotifyOfPropertyChange(() => ProxyPort);
            }
        }

        public string DownloadDirectory
        {
            get { return _SettingsManager.DirectorySetting.DownloadDirectory; }
            set
            {
                if (_SettingsManager.DirectorySetting.DownloadDirectory == value) return;
                _SettingsManager.DirectorySetting.DownloadDirectory = value;
                NotifyOfPropertyChange(() => DownloadDirectory);
            }
        }

        public string CacheDirectory
        {
            get { return _SettingsManager.DirectorySetting.CacheDirectory; }
            set
            {
                if (_SettingsManager.DirectorySetting.CacheDirectory == value) return;
                _SettingsManager.DirectorySetting.CacheDirectory = value;
                NotifyOfPropertyChange(() => CacheDirectory);
            }
        }

        public bool NotifyLocalTrackDetections
        {
            get { return _SettingsManager.BehaviorSetting.NotifyLocalTrackDetections; }
            set
            {
                if (_SettingsManager.BehaviorSetting.NotifyLocalTrackDetections == value) return;
                _SettingsManager.BehaviorSetting.NotifyLocalTrackDetections = value;
                NotifyOfPropertyChange(() => CacheDirectory);
            }
        }

        public bool CleanupAfterConversion
        {
            get { return _SettingsManager.BehaviorSetting.CleanupAfterConversion; }
            set
            {
                if (_SettingsManager.BehaviorSetting.CleanupAfterConversion == value) return;
                _SettingsManager.BehaviorSetting.CleanupAfterConversion = value;
                NotifyOfPropertyChange(() => CacheDirectory);
            }
        }

        public SettingsViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager) :
            base(eventAggregator, settingsManager)
        { }

        public void BrowseCacheDirectory()
        {
            var dialog = new FolderBrowserDialog();
            var dialogResult = dialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
                CacheDirectory = dialog.SelectedPath;
        }

        public void BrowseDownloadDirectory()
        {
            var dialog = new FolderBrowserDialog();
            var dialogResult = dialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
                DownloadDirectory = dialog.SelectedPath;
        }
    }
}
