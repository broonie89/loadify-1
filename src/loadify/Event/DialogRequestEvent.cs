namespace loadify.Event
{
    public abstract class DialogRequestEvent
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public DialogRequestEvent(string title, string content)
        {
            Title = title;
            Content = content;
        }
    }
}
