using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Model;
namespace loadify.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private SettingsModel _Settings;

        public bool UseProxy
        {
            get { return _Settings.UseProxy; }
            set
            {
                if (_Settings.UseProxy == value) return;
                _Settings.UseProxy = value;
                NotifyOfPropertyChange(() => UseProxy);
            }
        }

        public string ProxyIP
        {
            get { return _Settings.ProxyIP; }
            set
            {
                if (_Settings.ProxyIP == value) return;
                _Settings.ProxyIP = value;
                NotifyOfPropertyChange(() => ProxyIP);
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
            }
        }

        public SettingsViewModel(SettingsModel settings)
        {
            _Settings = settings;
        }

        public SettingsViewModel():
            this(new SettingsModel())
        { }
    }
}
