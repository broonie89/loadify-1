using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Properties;

namespace loadify.Configuration
{
    public class NETProxySetting : IProxySetting
    {
        public bool UseProxy
        {
            get { return Settings.Default.UseProxy; }
            set
            {
                Settings.Default.UseProxy = value;
                Settings.Default.Save();
            }
        }

        public string ProxyIp
        {
            get { return Settings.Default.ProxyIP; }
            set
            {
                Settings.Default.ProxyIP = value;
                Settings.Default.Save();
            }
        }

        public ushort ProxyPort
        {
            get { return Settings.Default.ProxyPort; }
            set
            {
                Settings.Default.ProxyPort = value;
                Settings.Default.Save();
            }
        }
    }
}
