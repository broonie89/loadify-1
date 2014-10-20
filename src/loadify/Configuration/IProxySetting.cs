using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Configuration
{
    public interface IProxySetting
    {
        bool UseProxy { get; set; }
        string ProxyIp { get; set; }
        ushort ProxyPort { get; set; }
    }
}
