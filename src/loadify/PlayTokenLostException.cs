using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify
{
    public class PlayTokenLostException : DownloadInterruptedException
    {
        public PlayTokenLostException(string msg):
            base(msg)
        { }
    }
}
