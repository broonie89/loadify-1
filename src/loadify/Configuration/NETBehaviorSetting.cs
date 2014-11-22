using loadify.Audio;
using loadify.Properties;

namespace loadify.Configuration
{
    public class NETBehaviorSetting : IBehaviorSetting
    {
        public bool NotifyLocalTrackDetections
        {
            get { return Settings.Default.NotifyLocalTrackDetections; }
            set
            {
                Settings.Default.NotifyLocalTrackDetections = value;
                Settings.Default.Save();
            }
        }

        public bool CleanupAfterConversion
        {
            get { return Settings.Default.CleanupAfterConversion; }
            set
            {
                Settings.Default.CleanupAfterConversion = value;
                Settings.Default.Save();
            }
        }

        public bool SkipOnDownloadFailures
        {
            get { return Settings.Default.SkipOnDownloadFailures; }
            set
            {
                Settings.Default.SkipOnDownloadFailures = value;
                Settings.Default.Save();
            }
        }

        public AudioProcessor AudioProcessor { get; set; }
        public AudioConverter AudioConverter { get; set; }
        public IAudioFileDescriptor AudioFileDescriptor { get; set; }
        public IDownloadPathConfigurator DownloadPathConfigurator { get; set; }

        public NETBehaviorSetting()
        {
            AudioProcessor = new WaveAudioProcessor();
            AudioConverter = new WaveToMp3Converter();
            AudioFileDescriptor = new Mp3FileDescriptor();
            DownloadPathConfigurator = new PlaylistRepositoryPathConfigurator();
        }
    }
}
