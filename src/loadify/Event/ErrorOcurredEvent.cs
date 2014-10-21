namespace loadify.Event
{
    public class ErrorOcurredEvent : DialogRequestEvent
    {
        public ErrorOcurredEvent(string title, string content)
            : base(title, content)
        { }
    }
}
