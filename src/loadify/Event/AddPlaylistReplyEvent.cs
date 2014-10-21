using loadify.Spotify;

namespace loadify.Event
{
    public class AddPlaylistReplyEvent : SessionEvent
    {
        public string Content { get; set; }

        public AddPlaylistReplyEvent(string content, LoadifySession session):
            base(session)
        {
            Content = content;
        }
    }
}
