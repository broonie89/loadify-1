using System;
using NAudio.Lame;
using NAudio.Wave;

namespace loadify.Audio
{
    public class WaveToMp3Converter : AudioConverter
    {
        public WaveToMp3Converter():
            base("mp3")
        { }

        public override string Convert(string inputFilePath, string outputFilePath)
        {
            using (var wavReader = new WaveFileReader(inputFilePath))
            using (var mp3Writer = new LameMP3FileWriter(outputFilePath, wavReader.WaveFormat, 128))
                wavReader.CopyTo(mp3Writer);

            return outputFilePath;
        }
    }
}
