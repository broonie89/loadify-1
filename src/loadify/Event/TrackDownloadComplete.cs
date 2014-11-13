using loadify.ViewModel;

namespace loadify.Event
{
    public class TrackDownloadComplete
    {
        public TrackViewModel Track { get; set; }

        public TrackDownloadComplete(TrackViewModel track)
        {
            Track = track;
        }
    }
}
