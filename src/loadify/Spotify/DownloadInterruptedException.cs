using System;

namespace loadify.Spotify
{
    public class DownloadInterruptedException : Exception
    {
        public DownloadInterruptedException(string msg):
            base(msg)
        { }
    }
}
