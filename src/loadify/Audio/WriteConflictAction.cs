namespace loadify.Audio
{
    /// <summary>
    /// Action that is executed if the audio file being written to already exists
    /// </summary>
    public enum WriteConflictAction
    {
        Skip,
        Overwrite,
        Rename,
        Notify
    }
}
