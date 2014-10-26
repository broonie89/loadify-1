namespace loadify.Audio
{
    public interface IAudioFileDescriptor
    {
        void Write(AudioFileMetaData audioFileMetaData, string inputFilePath);
        AudioFileMetaData Read(string inputFilePath);
    }
}
