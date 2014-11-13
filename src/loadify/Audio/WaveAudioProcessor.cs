using NAudio.Wave;

namespace loadify.Audio
{
    /// <summary>
    /// Audio processor that processes raw audio data into wave files
    /// </summary>
    public class WaveAudioProcessor : AudioProcessor
    {
        /// <summary>
        /// Writer used for writing binary data into the file
        /// </summary>
        public WaveFileWriter _WaveFileWriter;

        public WaveAudioProcessor(AudioMetaData audioMetaData) :
            base(audioMetaData, "wav")
        { }

         public WaveAudioProcessor() :
             this(new AudioMetaData())
        { }

         /// <summary>
         /// Initializes the processor and does preparation work
         /// </summary>
         /// <param name="outputFilePath"> Path where to store the converted file </param>
        public override void Start(string outputFilePath)
        {
            _WaveFileWriter = new WaveFileWriter(outputFilePath, new WaveFormat(AudioMetaData.SampleRate, AudioMetaData.BitsPerSample, AudioMetaData.Channels));
        }

        /// <summary>
        /// Processes the byte data and writes to the audio file
        /// </summary>
        /// <param name="data"> Raw audio data to write </param>
        public override void Process(byte[] data)
        {
            _WaveFileWriter.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Releases used resources and does cleanup work
        /// </summary>
        public override void Release()
        {
            if(_WaveFileWriter != null)
                _WaveFileWriter.Dispose();
        }
    }
}
