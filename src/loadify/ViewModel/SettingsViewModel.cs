using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using loadify.Event;
using loadify.Model;
namespace loadify.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private SettingsModel _Settings;

        public bool UseProxy
        {
            get { return (ProxyIP == null) ? false : ProxyIP.Length != 0 && ProxyPort != 0; }
        }

        public string ProxyIP
        {
            get { return _Settings.ProxyIP; }
            set
            {
                if (_Settings.ProxyIP == value) return;
                _Settings.ProxyIP = value;
                NotifyOfPropertyChange(() => ProxyIP);
                NotifyOfPropertyChange(() => UseProxy);
            }
        }

        public uint ProxyPort
        {
            get { return _Settings.ProxyPort; }
            set
            {
                if (_Settings.ProxyPort == value) return;
                _Settings.ProxyPort = value;
                NotifyOfPropertyChange(() => ProxyPort);
                NotifyOfPropertyChange(() => UseProxy);
            }
        }

        public SettingsViewModel(SettingsModel settings, IEventAggregator eventAggregator):
            base(eventAggregator)
        {
            _Settings = settings;
        }

        public SettingsViewModel(IEventAggregator eventAggregator):
            this(new SettingsModel(), eventAggregator)
        { }

        public void Validate(RoutedEventArgs args)
        {
            if (args.Source is TabItem)
            {
                args.Handled = true;
                return;
            }

            if (UseProxy)
            {
                if (!String.IsNullOrEmpty(ProxyIP))
                {
                    if (!Regex.IsMatch(ProxyIP,
                        @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$"))
                        _EventAggregator.PublishOnUIThread(new InvalidSettingEvent("The proxy IP address that was entered is not a valid IP address."));
                }
            }
        }
    }
}
