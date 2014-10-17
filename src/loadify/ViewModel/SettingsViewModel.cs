using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Caliburn.Micro;
using loadify.Event;
using loadify.Model;
using loadify.Properties;

namespace loadify.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        public bool UseProxy
        {
            get { return (ProxyIP == null) ? false : ProxyIP.Length != 0 && ProxyPort != 0; }
        }

        public string ProxyIP
        {
            get { return Settings.Default.ProxyIP; }
            set
            {
                if (Settings.Default.ProxyIP == value) return;
                Settings.Default.ProxyIP = value;
                Settings.Default.Save();
                NotifyOfPropertyChange(() => ProxyIP);
                NotifyOfPropertyChange(() => UseProxy);
            }
        }

        public ushort ProxyPort
        {
            get { return Settings.Default.ProxyPort; }
            set
            {
                if (Settings.Default.ProxyPort == value) return;
                Settings.Default.ProxyPort = value;
                Settings.Default.Save();
                NotifyOfPropertyChange(() => ProxyPort);
                NotifyOfPropertyChange(() => UseProxy);
            }
        }

        public string DownloadDirectory
        {
            get { return Settings.Default.DownloadDirectory; }
            set
            {
                if (Settings.Default.DownloadDirectory == value) return;
                Settings.Default.DownloadDirectory = value;
                Settings.Default.Save();
                NotifyOfPropertyChange(() => DownloadDirectory);
            }
        }

        public string CacheDirectory
        {
            get { return Settings.Default.CacheDirectory; }
            set
            {
                if (Settings.Default.CacheDirectory == value) return;
                Settings.Default.CacheDirectory = value;
                Settings.Default.Save();
                NotifyOfPropertyChange(() => CacheDirectory);
            }
        }

        public SettingsViewModel(IEventAggregator eventAggregator):
            base(eventAggregator)
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

        public void Validate(RoutedEventArgs args)
        {
            if (args.Source is TabItem)
            {
                args.Handled = true;
                return;
            }

            if (UseProxy)
            {
                if (!Regex.IsMatch(ProxyIP, @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$"))
                    _EventAggregator.PublishOnUIThread(
                        new ErrorOcurredEvent("Settings Error", "The proxy IP address that was entered is not a valid IP address."));
            }
        }
    }
}
