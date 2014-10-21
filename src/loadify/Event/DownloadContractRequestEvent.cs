using loadify.Spotify;

namespace loadify.Event
{
    public class DownloadContractRequestEvent : SessionEvent
    {
        public DownloadContractRequestEvent(LoadifySession session)
            : base(session)
        { }
    }
}
