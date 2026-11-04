namespace GameRules;

internal static class SettingTags
{
    /// <summary>
    /// Read by Exiled's Toolkit: each world may keep its own value of this live setting. Settings that apply only at
    /// the next game start are not tagged, as a world could not change them.
    /// </summary>
    public const string WorldRule = "ModManager.WorldRule";
}
