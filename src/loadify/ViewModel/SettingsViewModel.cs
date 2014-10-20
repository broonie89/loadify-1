using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Caliburn.Micro;
using loadify.Audio;
using loadify.Configuration;
using loadify.Event;
using loadify.Model;
using loadify.Properties;

namespace loadify.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private IConnectionSetting _ConnectionSetting;
        public IConnectionSetting ConnectionSetting
        {
            get { return _ConnectionSetting; }
            set
            {
                _ConnectionSetting = value;
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IConnectionSetting>(ConnectionSetting));
            }
        }

        public bool UseProxy
        {
            get { return ConnectionSetting.UseProxy; }
            set
            {
                if (ConnectionSetting.UseProxy == value) return;
                ConnectionSetting.UseProxy = value;
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IConnectionSetting>(ConnectionSetting));
                NotifyOfPropertyChange(() => UseProxy);
            }
        }

        public string ProxyIp
        {
            get { return ConnectionSetting.ProxyIp; }
            set
            {
                if (ConnectionSetting.ProxyIp == value) return;
                ConnectionSetting.ProxyIp = value;
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IConnectionSetting>(ConnectionSetting));
                NotifyOfPropertyChange(() => ProxyIp);
            }
        }

        public ushort ProxyPort
        {
            get { return ConnectionSetting.ProxyPort; }
            set
            {
                if (ConnectionSetting.ProxyPort == value) return;
                ConnectionSetting.ProxyPort = value;
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IConnectionSetting>(ConnectionSetting));
                NotifyOfPropertyChange(() => ProxyPort);
            }
        }

        private IDirectorySetting _DirectorySetting;
        public IDirectorySetting DirectorySetting
        {
            get { return _DirectorySetting; }
            set
            {
                _DirectorySetting = value;
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IDirectorySetting>(DirectorySetting));
            }
        }

        public string DownloadDirectory
        {
            get { return _DirectorySetting.DownloadDirectory; }
            set
            {
                if (_DirectorySetting.DownloadDirectory == value) return;
                _DirectorySetting.DownloadDirectory = value;
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IDirectorySetting>(DirectorySetting));
                NotifyOfPropertyChange(() => DownloadDirectory);
            }
        }

        public string CacheDirectory
        {
            get { return _DirectorySetting.CacheDirectory; }
            set
            {
                if (_DirectorySetting.CacheDirectory == value) return;
                _DirectorySetting.CacheDirectory = value;
                _EventAggregator.PublishOnUIThread(new SettingChangedEvent<IDirectorySetting>(DirectorySetting));
                NotifyOfPropertyChange(() => CacheDirectory);
            }
        }

        public WriteConflictAction WriteConflictAction
        {
            get { return (WriteConflictAction) Enum.Parse(typeof(WriteConflictAction), Settings.Default.WriteConflictAction); }
            set
            {
                if (Settings.Default.WriteConflictAction == value.ToString()) return;
                Settings.Default.WriteConflictAction = value.ToString();
                Settings.Default.Save();
                NotifyOfPropertyChange(() => WriteConflictAction);
            }
        }

        public List<WriteConflictAction> WriteConflictActions
        {
            get { return Enum.GetValues(typeof(WriteConflictAction)).Cast<WriteConflictAction>().ToList(); }
        }

        public SettingsViewModel(IEventAggregator eventAggregator, IDirectorySetting directorySetting, IConnectionSetting connectionSetting) :
            base(eventAggregator)
        {
            DirectorySetting = directorySetting;
            ConnectionSetting = connectionSetting;
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
