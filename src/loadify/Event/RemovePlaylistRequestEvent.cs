using loadify.ViewModel;

namespace loadify.Event
{
    public class RemovePlaylistRequestEvent
    {
        public PlaylistViewModel Playlist { get; set; }

        public RemovePlaylistRequestEvent(PlaylistViewModel playlist)
        {
            Playlist = playlist;
        }
    }
}
