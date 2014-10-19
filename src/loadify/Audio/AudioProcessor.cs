namespace loadify.Audio
{
    public abstract class AudioProcessor
    {
        public string OutputDirectory { get; set; }
        public string OutputFileName { get; set; }
        public string OutputFilePath { get; protected set; }
        public AudioMetaData AudioMetaData { get; set; }

        public AudioProcessor(string outputDirectory, string outputFileName, AudioMetaData audioMetaData)
        {
            OutputDirectory = outputDirectory;
            OutputFileName = outputFileName;
            AudioMetaData = audioMetaData;
        }

        public AudioProcessor(string outputDirectory, string outputFileName):
            this(outputDirectory, outputFileName, new AudioMetaData())
        { }

        public abstract void Process(byte[] audioData);

        public virtual void Release() { }
    }
}
