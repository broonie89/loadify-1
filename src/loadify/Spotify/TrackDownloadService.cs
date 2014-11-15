using System;
using System.IO;
using System.Runtime.InteropServices;
using loadify.Audio;
using loadify.Configuration;
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
        public Mp3MetaData Mp3MetaData { get; set; }
        public AudioMetaData AudioMetaData { get; set; }
        public AudioProcessor AudioProcessor { get; set; }
        public AudioConverter AudioConverter { get; set; }
        public IAudioFileDescriptor AudioFileDescriptor { get; set; }
        public IDownloadPathConfigurator DownloadPathConfigurator { get; set; }
        public bool Cleanup { get; set; }
        public TrackModel Track { get; set; }
        public Action<double> DownloadProgressUpdated = progress => { };

        private Statistic _Statistic = new Statistic();

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

        public TrackDownloadService(TrackModel track, AudioProcessor audioProcessor, IDownloadPathConfigurator downloadPathConfigurator)
        {
            Track = track;
            AudioProcessor = audioProcessor;
            DownloadPathConfigurator = downloadPathConfigurator;
            AudioMetaData = new AudioMetaData();
            Mp3MetaData = new Mp3MetaData();
            Cleanup = true;
            OutputDirectory = "download";
        }

        public void Start()
        {
            _Statistic = new Statistic(Track.Duration);
            AudioProcessor.Start(DownloadPathConfigurator.Configure(OutputDirectory, AudioProcessor.TargetFileExtension, Track));
            Active = true;
        }

        public void Stop()
        {
            AudioProcessor.Release();
            Active = false;
        }

        public void ProcessInput(AudioFormat format, IntPtr frames, int numFrames)
        {
            AudioMetaData.SampleRate = format.sample_rate;
            AudioMetaData.Channels = format.channels;

            var size = numFrames * format.channels * 2;
            var buffer = new byte[size];
            Marshal.Copy(frames, buffer, 0, size);
            AudioProcessor.Process(buffer);

            _Statistic.Processings++;
            _Statistic.AverageFrameSize = (_Statistic.AverageFrameSize + size) / 2;

            DownloadProgressUpdated(Progress);
        }

        public void Finish()
        {
            Stop();

            var processorOutputPath = DownloadPathConfigurator.Configure(OutputDirectory, AudioProcessor.TargetFileExtension, Track);
            var converterOutputPath = DownloadPathConfigurator.Configure(OutputDirectory, AudioConverter.TargetFileExtension, Track);

            if (AudioConverter != null)
                AudioConverter.Convert(processorOutputPath, converterOutputPath);

            if (AudioFileDescriptor != null)
                AudioFileDescriptor.Write(Mp3MetaData, (AudioConverter != null) ? converterOutputPath : processorOutputPath);

            if (Cleanup)
            {
                if (File.Exists(processorOutputPath))
                    File.Delete(processorOutputPath);
            }
        }

        public void Cancel(CancellationReason reason)
        {
            Cancellation = reason;
            Stop();
        }
    }
}
