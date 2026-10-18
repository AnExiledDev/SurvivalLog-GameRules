using System;
using GameCore.HotUpdate;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Sets the restart cap. Every mode reads it through GameKey.GlobalSetting_RebirthMaxChances, whose getter fills the
/// static field _GlobalSetting_RebirthMaxChances from the GlobalSetting table only while the field is 0 and trusts it
/// from then on, so the cap is written into that field, and into the table entry the getter copies (Params1) so a
/// reset field reloads the mod's cap. The restarts used live on the loaded save (GameSaveData.RebirthChancesUsed), so
/// the field is checked every second and moved whenever a restart is spent or a different run loads.
/// </summary>
internal sealed class RestartCapField
{
    private const float CheckSeconds = 1f;

    private readonly RestartKeeper _keeper;
    private readonly Action<string> _log;
    private float _nextCheckAt;
    private bool _hasLoggedPutBack;

    public RestartCapField(RestartKeeper keeper, Action<string> log)
    {
        _keeper = keeper;
        _log = log;
    }

    public void Tick(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        var current = GameKey._GlobalSetting_RebirthMaxChances;

        if (current <= 0) current = LoadGameCap();

        if (_keeper.Next(current, RestartsUsed()) is not { } write) return;

        Write(write.Max);
        Report(write);
    }

    private static int? RestartsUsed() => GameSingleton.Existing<GameSaveManager>()?.gameSave?.RebirthChancesUsed;

    private static void Write(int max)
    {
        GameKey._GlobalSetting_RebirthMaxChances = max;

        if (TableEntry() is { } entry) entry._Params1_k__BackingField = max;
    }

    /// <summary>
    /// Has the game's getter fill the field. The getter looks the cap up in ConfigManager's GlobalSetting table, which
    /// throws while the table is not loaded and logs an error for a missing key, so it is only asked once the table
    /// holds the key.
    /// </summary>
    private static int LoadGameCap() => TableEntry() == null ? 0 : GameKey.GlobalSetting_RebirthMaxChances;

    private static Config_GlobalSetting? TableEntry()
    {
        var table = GameSingleton.Existing<ConfigManager>()?._Config_GlobalSetting_Dict;
        var key = GameKey.Key_GlobalSetting_RebirthMaxChances;

        return table != null && key != null && table.TryGetValue(key, out var entry) ? entry : null;
    }

    /// <summary>The cap moves with every restart spent, so routine moves are not logged; a put-back is, once.</summary>
    private void Report(RestartWrite write)
    {
        if (write.IsPutBack)
        {
            if (!_hasLoggedPutBack) _log($"the restart cap was changed by something else; put back to {write.Max} (reported once)");

            _hasLoggedPutBack = true;

            return;
        }

        _log($"restart cap is now {write.Max}, so {write.GameMax} restarts stay available (the game's own cap is {write.GameMax})");
    }
}
