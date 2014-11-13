namespace loadify.Audio
{
    /// <summary>
    /// Base class for audio processors that convert raw binary data into audio files
    /// </summary>
    public abstract class AudioProcessor
    {
        /// <summary>
        /// Metadata about the audio stream
        /// </summary>
        public AudioMetaData AudioMetaData { get; set; }

        /// <summary>
        /// The file extension of the resulting processed file
        /// </summary>
        public string TargetFileExtension { get; set; }

        public AudioProcessor(AudioMetaData audioMetaData, string targetFileExtension)
        {
            AudioMetaData = audioMetaData;
            TargetFileExtension = targetFileExtension;
        }

        /// <summary>
        /// Initializes the processor and does preparation work
        /// </summary>
        /// <param name="outputFilePath"> Path where to store the converted file </param>
        public abstract void Start(string outputFilePath);

        /// <summary>
        /// Processes the byte data and writes to the audio file
        /// </summary>
        /// <param name="data"> Raw audio data to write </param>
        public abstract void Process(byte[] data);

        /// <summary>
        /// Releases used resources and does cleanup work
        /// </summary>
        public virtual void Release() { }
    }
}
