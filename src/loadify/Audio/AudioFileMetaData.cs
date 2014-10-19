namespace loadify.Audio
{
    public class AudioFileMetaData
    {
        public string Title { get; set; }
        public string Artists { get; set; }
        public string Album { get; set; }
        public int Year { get; set; }
        public byte[] Cover { get; set; }

        public AudioFileMetaData()
        { }
    }
}
