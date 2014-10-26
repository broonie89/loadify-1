namespace loadify.Audio
{
    public abstract class AudioConverter
    {
        public string TargetFileExtension { get; set; }

        public AudioConverter(string targetFileExtension)
        {
            TargetFileExtension = targetFileExtension;
        }

        public abstract string Convert(string inputFilePath, string outputFilePath);
        public virtual void Release() { }
    }
}
