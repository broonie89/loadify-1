namespace loadify.Event
{
    public class UserInputRequestEvent : DialogRequestEvent
    {
        public UserInputRequestEvent(string title, string content):
            base(title, content)
        { }
    }
}
