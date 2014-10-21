namespace loadify.Event
{
    public class SettingChangedEvent<T>
    {
        public T Setting { get; set; }

        public SettingChangedEvent(T setting)
        {
            Setting = setting;
        }
    }
}
