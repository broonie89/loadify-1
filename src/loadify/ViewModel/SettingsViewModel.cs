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
        private readonly ISettingsManager _SettingsManager;

        public bool UseProxy
        {
            get { return _SettingsManager.ConnectionSetting.UseProxy; }
            set
            {
                if (_SettingsManager.ConnectionSetting.UseProxy == value) return;
                _SettingsManager.ConnectionSetting.UseProxy = value;
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IConnectionSetting>(_SettingsManager.ConnectionSetting));
                NotifyOfPropertyChange(() => UseProxy);
            }
        }

        public string ProxyIp
        {
            get { return _SettingsManager.ConnectionSetting.ProxyIp; }
            set
            {
                if (_SettingsManager.ConnectionSetting.ProxyIp == value) return;
                _SettingsManager.ConnectionSetting.ProxyIp = value;
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IConnectionSetting>(_SettingsManager.ConnectionSetting));
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
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IConnectionSetting>(_SettingsManager.ConnectionSetting));
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
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IDirectorySetting>(_SettingsManager.DirectorySetting));
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
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IDirectorySetting>(_SettingsManager.DirectorySetting));
                NotifyOfPropertyChange(() => CacheDirectory);
            }
        }

        public WriteConflictAction WriteConflictAction
        {
            get { return _SettingsManager.BehaviorSetting.WriteConflictAction.ConvertedValue; }
            set
            {

                if (_SettingsManager.BehaviorSetting.WriteConflictAction.ConvertedValue == value) return;
                _SettingsManager.BehaviorSetting.WriteConflictAction.ConvertedValue = value;
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IBehaviorSetting>(_SettingsManager.BehaviorSetting));
                NotifyOfPropertyChange(() => WriteConflictAction);
            }
        }

        public List<WriteConflictAction> WriteConflictActions
        {
            get { return Enum.GetValues(typeof(WriteConflictAction)).Cast<WriteConflictAction>().ToList(); }
        }

        public SettingsViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager) :
            base(eventAggregator)
        {
            _SettingsManager = settingsManager;
            _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IDirectorySetting>(_SettingsManager.DirectorySetting));
        }

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

        public void Validate(RoutedEventArgs args)
        {
            if (args.Source is TabItem)
            {
                args.Handled = true;
                return;
            }

            if (UseProxy)
            {
                if (!Regex.IsMatch(ProxyIp, @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$"))
                    _EventAggregator.PublishOnUIThread(
                        new ErrorOcurredEvent("Settings Error", "The proxy IP address that was entered is not a valid IP address."));
            }
        }
    }
}
