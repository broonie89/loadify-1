using loadify.Spotify;

namespace loadify.Event
{
    public class AddPlaylistReplyEvent : SessionEvent
    {
        public string Content { get; set; }

        /// <summary>
        /// If set to true, the playlist will be added permanently to the logged-in users spotify account
        /// </summary>
        public bool Permanent { get; set; }

        public AddPlaylistReplyEvent(string content, LoadifySession session, bool permanent = false):
            base(session)
        {
            Content = content;
            Permanent = permanent;
        }
    }
}
