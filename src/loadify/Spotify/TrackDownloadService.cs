using System;
using System.Runtime.InteropServices;
using loadify.Audio;
using loadify.Model;
using SpotifySharp;

namespace loadify.Spotify
{
    public class TrackDownloadService
    {
        private class Statistic
        {
            public TimeSpan TargetDuration { get; set; }
            public uint Processings { get; set; }
            public double AverageFrameSize { get; set; }

            public Statistic(TimeSpan targetDuration)
            {
                TargetDuration = targetDuration;
            }

            public Statistic()
                : this(new TimeSpan())
            { }
        }


        public enum CancellationReason
        {
            None,
            UserInteraction,
            PlayTokenLost,
            Unknown,
            ConnectionLost
        };

        public bool Active { get; set; }
        public CancellationReason Cancellation { get; set; }
        public string OutputDirectory { get; set; }
        public string OutputFileName { get; set; }
        public Mp3MetaData Mp3MetaData { get; set; }
        public AudioMetaData AudioMetaData { get; set; }
        public AudioProcessor AudioProcessor { get; set; }
        public AudioConverter AudioConverter { get; set; }
        public IAudioFileDescriptor AudioFileDescriptor { get; set; }

        private Statistic _Statistic = new Statistic();
        private readonly Action<double> _DownloadProgressUpdatedCallback = progress => { };

        public double Progress
        {
            get
            {
                var trackDuration = _Statistic.TargetDuration.TotalMilliseconds;
                return (trackDuration != 0)
                        ? 100 / _Statistic.TargetDuration.TotalMilliseconds * (46.4 * _Statistic.Processings)
                        : 100;
            }
        }

        public TrackDownloadService(string outputDirectory, string outputFileName,
                                    AudioProcessor audioProcessor, AudioConverter audioConverter, IAudioFileDescriptor audioFileDescriptor,
                                    Mp3MetaData mp3MetaData,
                                    Action<double> downloadProgressUpdatedCallback)
        {
            OutputDirectory = outputDirectory;
            OutputFileName = outputFileName;
            AudioProcessor = audioProcessor;
            AudioConverter = audioConverter;
            AudioFileDescriptor = audioFileDescriptor;
            Mp3MetaData = mp3MetaData;
            _DownloadProgressUpdatedCallback = downloadProgressUpdatedCallback;
            AudioMetaData = new AudioMetaData();
        }

        public void Start(TrackModel track)
        {
            _Statistic = new Statistic(track.Duration);
            AudioProcessor.Start(String.Format("{0}/{1}.{2}", OutputDirectory, OutputFileName, AudioProcessor.TargetFileExtension));
            Active = true;
        }

        public void Stop()
        {
            AudioProcessor.Release();
            Active = false;
        }

        public void ProcessInput(AudioFormat format, IntPtr frames, int num_frames)
        {
            AudioMetaData.SampleRate = format.sample_rate;
            AudioMetaData.Channels = format.channels;

            var size = num_frames * format.channels * 2;
            var buffer = new byte[size];
            Marshal.Copy(frames, buffer, 0, size);
            AudioProcessor.Process(buffer);

            _Statistic.Processings++;
            _Statistic.AverageFrameSize = (_Statistic.AverageFrameSize + size) / 2;

            _DownloadProgressUpdatedCallback(Progress);
        }

        public void Finish()
        {
            Stop();

            var outputFilePath = String.Format("{0}/{1}.{2}", OutputDirectory, OutputFileName, AudioProcessor.TargetFileExtension);
            if (AudioConverter != null)
                AudioConverter.Convert(outputFilePath, String.Format("{0}/{1}.{2}", 
                                                        OutputDirectory, 
                                                        OutputFileName, 
                                                        AudioConverter.TargetFileExtension));

            if (AudioFileDescriptor != null)
                AudioFileDescriptor.Write(Mp3MetaData, outputFilePath);
        }

        public void Cancel(CancellationReason reason)
        {
            Cancellation = reason;
            Stop();
        }
    }
}
