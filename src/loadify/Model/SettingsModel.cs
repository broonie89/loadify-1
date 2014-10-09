using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Model
{
    public class SettingsModel
    {
        public bool UseProxy { get; set; }
        public string ProxyIP { get; set; }
        public uint ProxyPort { get; set; }

        public SettingsModel()
        {
            ProxyPort = 80;
        }
    }
}
