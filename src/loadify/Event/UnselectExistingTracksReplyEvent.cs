namespace loadify.Event
{
    public class UnselectExistingTracksReplyEvent
    {
        public bool Unselect { get; set; }

        public UnselectExistingTracksReplyEvent(bool unselect)
        {
            Unselect = unselect;
        }
    }
}
