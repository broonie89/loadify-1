using loadify.Audio;
using loadify.Properties;

namespace loadify.Configuration
{
    public class NETBehaviorSetting : IBehaviorSetting
    {
        public EnumSetting<WriteConflictAction> WriteConflictAction
        {
            get { return new EnumSetting<WriteConflictAction>(Settings.Default.WriteConflictAction); }
            set
            {
                Settings.Default.WriteConflictAction = value.RawValue;
                Settings.Default.Save();
            }
        }

        public AudioProcessor AudioProcessor { get; set; }
        public AudioConverter AudioConverter { get; set; }
        public IAudioFileDescriptor AudioFileDescriptor { get; set; }

        public NETBehaviorSetting()
        {
            AudioProcessor = new WaveAudioProcessor();
            AudioConverter = new WaveToMp3Converter();
            AudioFileDescriptor = new Mp3FileDescriptor();
        }
    }
}
