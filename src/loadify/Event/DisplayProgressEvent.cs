namespace loadify.Event
{
    public class DisplayProgressEvent
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public DisplayProgressEvent(string title, string content)
        {
            Title = title;
            Content = content;
        }
    }
}
