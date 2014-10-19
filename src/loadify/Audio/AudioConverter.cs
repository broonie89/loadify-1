namespace loadify.Audio
{
    public abstract class AudioConverter
    {
        public string OutputDirectory { get; set; }
        public string OutputFileName { get; set; }

        public AudioConverter(string outputDirectory, string outputFileName)
        {
            OutputDirectory = outputDirectory;
            OutputFileName = outputFileName;
        }

        public abstract string Convert(string inputFilePath);
        public virtual void Release() { }
    }
}
