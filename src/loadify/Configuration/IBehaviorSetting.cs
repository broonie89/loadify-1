using loadify.Audio;

namespace loadify.Configuration
{
    public interface IBehaviorSetting
    {
        bool NotifyLocalTrackDetections { get; set; }
        bool CleanupAfterConversion { get; set; }
        AudioProcessor AudioProcessor { get; set; }
        AudioConverter AudioConverter { get; set; }
        IAudioFileDescriptor AudioFileDescriptor { get; set; }
        IDownloadPathConfigurator DownloadPathConfigurator { get; set; }
        bool SkipOnDownloadFailures { get; set; }
    }
}
