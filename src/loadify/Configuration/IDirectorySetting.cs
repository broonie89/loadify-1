using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Configuration
{
    public interface IDirectorySetting
    {
        string DownloadDirectory { get; set; }
        string CacheDirectory { get; set; }
    }
}
