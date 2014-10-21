namespace loadify.Configuration
{
    public interface IDirectorySetting
    {
        string DownloadDirectory { get; set; }
        string CacheDirectory { get; set; }
    }
}
