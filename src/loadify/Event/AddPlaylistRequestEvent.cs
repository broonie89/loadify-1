namespace loadify.Event
{
    public class AddPlaylistRequestEvent : UserInputRequestEvent
    {
        public AddPlaylistRequestEvent(string title, string content):
            base(title, content)
        { }
    }
}
