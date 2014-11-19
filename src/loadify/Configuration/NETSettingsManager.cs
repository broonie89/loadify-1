namespace loadify.Configuration
{
    public class NETSettingsManager : ISettingsManager
    {
        public IDirectorySetting DirectorySetting { get; set; }
        public IBehaviorSetting BehaviorSetting { get; set; }
        public ICredentialsSetting CredentialsSetting { get; set; }

        public NETSettingsManager()
        {
            DirectorySetting = new NETDirectorySetting();
            BehaviorSetting = new NETBehaviorSetting();
            CredentialsSetting = new NETCredentialsSetting();
        }
    }
}
