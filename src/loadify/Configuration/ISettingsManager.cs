namespace loadify.Configuration
{
    public interface ISettingsManager
    {
        IDirectorySetting DirectorySetting { get; set; }
        IConnectionSetting ConnectionSetting { get; set; }
        IBehaviorSetting BehaviorSetting { get; set; }
        ICredentialsSetting CredentialsSetting { get; set; }
    }
}
