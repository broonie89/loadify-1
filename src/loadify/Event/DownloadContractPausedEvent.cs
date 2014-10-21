namespace loadify.Event
{
    public class DownloadContractPausedEvent
    {
        public string Reason { get; set; }
        public int DownloadIndex { get; set; }

        public DownloadContractPausedEvent(string reason = null, int downloadIndex = 0)
        {
            Reason = reason;
            DownloadIndex = downloadIndex;
        }
    }
}
