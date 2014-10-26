using System;
using NAudio.Wave;

namespace loadify.Audio
{
    public class WaveAudioProcessor : AudioProcessor
    {
        public WaveFileWriter _WaveFileWriter;

        public WaveAudioProcessor(AudioMetaData audioMetaData) :
            base(audioMetaData, "wav")
        { }

         public WaveAudioProcessor() :
             this(new AudioMetaData())
        { }

        public override void Start(string outputFilePath)
        {
            _WaveFileWriter = new WaveFileWriter(outputFilePath, new WaveFormat(AudioMetaData.SampleRate, AudioMetaData.BitRate, AudioMetaData.Channels));
        }

        public override void Process(byte[] audioData)
        {
            _WaveFileWriter.Write(audioData, 0, audioData.Length);
        }

        public override void Release()
        {
            if(_WaveFileWriter != null)
                _WaveFileWriter.Dispose();
        }
    }
}
