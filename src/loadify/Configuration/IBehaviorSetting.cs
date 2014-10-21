using loadify.Audio;

namespace loadify.Configuration
{
    public interface IBehaviorSetting
    {
        EnumSetting<WriteConflictAction> WriteConflictAction { get; set; }  
    }
}
