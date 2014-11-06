using loadify.ViewModel;

namespace loadify.Event
{
    public class AddTrackRequestEvent
    {
        public PlaylistViewModel Playlist { get; set; }

        public AddTrackRequestEvent(PlaylistViewModel playlist)
        {
            Playlist = playlist;
        }
    }
}
