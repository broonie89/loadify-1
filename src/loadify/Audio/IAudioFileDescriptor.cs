namespace loadify.Audio
{
    public interface IAudioFileDescriptor
    {
        void Write(Mp3MetaData mp3MetaData, string inputFilePath);
        Mp3MetaData Read(string inputFilePath);
    }
}
