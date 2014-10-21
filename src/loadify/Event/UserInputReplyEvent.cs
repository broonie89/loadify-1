namespace loadify.Event
{
    public class UserInputReplyEvent
    {
        public string Content { get; set; }

        public UserInputReplyEvent(string content)
        {
            Content = content;
        }
    }
}
