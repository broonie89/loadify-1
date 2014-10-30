using loadify.Audio;

namespace loadify.Configuration
{
    public interface IBehaviorSetting
    {
        EnumSetting<WriteConflictAction> WriteConflictAction { get; set; }
        AudioProcessor AudioProcessor { get; set; }
        AudioConverter AudioConverter { get; set; }
        IAudioFileDescriptor AudioFileDescriptor { get; set; }
    }
}
