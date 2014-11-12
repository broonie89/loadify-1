using loadify.Model;

namespace loadify.Configuration
{
    public interface IDownloadPathConfigurator
    {
        string Configure(string basePath, string targetFileExtension, TrackModel track);
    }
}
