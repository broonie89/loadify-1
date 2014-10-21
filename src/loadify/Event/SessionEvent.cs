using loadify.Spotify;

namespace loadify.Event
{
    public class SessionEvent
    {
        public LoadifySession Session { get; set; }

        public SessionEvent(LoadifySession session)
        {
            Session = session;
        }
    }
}
