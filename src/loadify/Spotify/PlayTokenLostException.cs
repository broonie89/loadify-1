namespace loadify.Spotify
{
    public class PlayTokenLostException : DownloadInterruptedException
    {
        public PlayTokenLostException(string msg):
            base(msg)
        { }
    }
}
