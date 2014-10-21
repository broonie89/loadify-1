using loadify.Properties;

namespace loadify.Configuration
{
    public class NETDirectorySetting : IDirectorySetting
    {
        public string DownloadDirectory
        {
            get { return Settings.Default.DownloadDirectory; }
            set
            {
                Settings.Default.DownloadDirectory = value;
                Settings.Default.Save();
            }
        }

        public string CacheDirectory
        {
            get { return Settings.Default.CacheDirectory; }
            set
            {
                Settings.Default.CacheDirectory = value;
                Settings.Default.Save();
            }
        }
    }
}
