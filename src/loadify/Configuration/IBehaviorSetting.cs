using loadify.Audio;

namespace loadify.Configuration
{
    public interface IBehaviorSetting
    {
        bool NotifyLocalTrackDetections { get; set; }
        AudioProcessor AudioProcessor { get; set; }
        AudioConverter AudioConverter { get; set; }
        IAudioFileDescriptor AudioFileDescriptor { get; set; }
    }
}
