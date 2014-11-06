namespace loadify.Audio
{
    /// <summary>
    /// Metadata structure containing data that may be serialized into mp3 files using ID3 tags. 
    /// </summary>
    public class Mp3MetaData
    {
        public string Title { get; set; }
        public string Artists { get; set; }
        public string Album { get; set; }
        public int Year { get; set; }
        public byte[] Cover { get; set; }
    }
}
