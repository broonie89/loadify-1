using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify
{
    public class DownloadInterruptedException : Exception
    {
        public DownloadInterruptedException(string msg):
            base(msg)
        { }
    }
}
