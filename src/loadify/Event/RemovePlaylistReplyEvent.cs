using loadify.Spotify;
using loadify.ViewModel;

namespace loadify.Event
{
    public class RemovePlaylistReplyEvent
    {
        public PlaylistViewModel Playlist { get; set; }
        public LoadifySession Session { get; set; }

        /// <summary>
        /// If set to true, the playlist will be removed permanently from the logged-in users spotify account
        /// </summary>
        public bool Permanent { get; set; }

        public RemovePlaylistReplyEvent(LoadifySession session, PlaylistViewModel playlist, bool permanent = false)
        {
            Session = session;
            Playlist = playlist;
            Permanent = permanent;
        }
    }
}
