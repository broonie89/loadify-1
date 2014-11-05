namespace loadify.Event
{
    public class NotificationEvent
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public NotificationEvent(string title, string content)
        {
            Title = title;
            Content = content;
        }
    }
}
