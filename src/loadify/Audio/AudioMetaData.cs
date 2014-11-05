namespace loadify.Audio
{
    /// <summary>
    /// Metadata about audio data containing information about the data such as the sample rate or bit rate
    /// </summary>
    public class AudioMetaData
    {
        public int SampleRate { get; set; }
        public int BitRate { get; set; }
        public int Channels { get; set; }

        public AudioMetaData(int sampleRate = 44000, int bitRate = 16, int channels = 2)
        {
            SampleRate = sampleRate;
            BitRate = bitRate;
            Channels = channels;
        }
    }
}
