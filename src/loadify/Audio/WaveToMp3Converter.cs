using System;
using NAudio.Lame;
using NAudio.Wave;

namespace loadify.Audio
{
    /// <summary>
    /// Audio converter for converting wave audio files to valid MP3 audio files
    /// </summary>
    public class WaveToMp3Converter : AudioConverter
    {
        public WaveToMp3Converter():
            base("mp3")
        { }

        /// <summary>
        /// Converts the specified audio file to another format
        /// </summary>
        /// <param name="filePath"> Path to the audio file being converted </param>
        /// <param name="outputFilePath"> Path where to store the converted file </param>
        public override void Convert(string filePath, string outputFilePath)
        {
            using (var wavReader = new WaveFileReader(filePath))
            using (var mp3Writer = new LameMP3FileWriter(outputFilePath, wavReader.WaveFormat, 128))
                wavReader.CopyTo(mp3Writer);
        }
    }
}
