using System;
using NAudio.Wave;

namespace loadify.Audio
{
    public class WaveAudioProcessor : AudioProcessor
    {
        private WaveFileWriter _WaveFileWriter;

        public WaveAudioProcessor(string outputDirectory, string outputFileName, AudioMetaData audioMetaData) :
            base(outputDirectory, outputFileName, audioMetaData)
        {
            OutputFilePath = String.Format("{0}/{1}.wav", OutputDirectory, OutputFileName);
            _WaveFileWriter = new WaveFileWriter(OutputFilePath, new WaveFormat(audioMetaData.SampleRate, audioMetaData.BitRate, audioMetaData.Channels));
        }

         public WaveAudioProcessor(string outputDirectory, string outputFileName) :
             this(outputDirectory, outputFileName, new AudioMetaData())
        { }

        public override void Process(byte[] audioData)
        {
            if (_WaveFileWriter != null)
                _WaveFileWriter.Write(audioData, 0, audioData.Length);
        }

        public override void Release()
        {
            _WaveFileWriter.Dispose();
        }
    }
}
