using System.IO;

namespace loadify.Audio
{
    public abstract class AudioProcessor
    {
        public AudioMetaData AudioMetaData { get; set; }
        public string TargetFileExtension { get; set; }

        public AudioProcessor(AudioMetaData audioMetaData, string targetFileExtension)
        {
            AudioMetaData = audioMetaData;
            TargetFileExtension = targetFileExtension;
        }

        public abstract void Start(string outputFilePath);
        public abstract void Process(byte[] audioData);
        public virtual void Release() { }
    }
}
