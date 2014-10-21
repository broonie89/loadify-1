namespace loadify.Configuration
{
    public interface IConnectionSetting
    {
        bool UseProxy { get; set; }
        string ProxyIp { get; set; }
        ushort ProxyPort { get; set; }
    }
}
