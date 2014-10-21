using loadify.Spotify;

namespace loadify.Event
{
    public class DataRefreshAuthorizedEvent : SessionEvent
    {
        public DataRefreshAuthorizedEvent(LoadifySession session) :
            base(session)
        { }
    }
}
