using loadify.ViewModel;

namespace loadify.Event
{
    public class AddTrackRequestEvent : UserInputRequestEvent
    {
        public PlaylistViewModel Playlist { get; set; }

        public AddTrackRequestEvent(string title, string content, PlaylistViewModel playlist) :
            base(title, content)
        {
            Playlist = playlist;
        }
    }
}
