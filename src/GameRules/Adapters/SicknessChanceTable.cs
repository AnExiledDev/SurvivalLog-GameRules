using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Writes the chance of catching Wind Chill or a High Temperature in a cold wave without heat. Each is FloatValue0 of
/// its row in the game's effect table (Config_Effect), read every time the game rolls for it, so a changed row takes
/// effect at the next roll. Checked every two seconds: the table loads after the plugin, and a reload builds fresh
/// rows holding the game's values. The keeper remembers the game's chance, so a setting put back to 1 restores it.
/// </summary>
internal sealed class SicknessChanceTable
{
    private const float CheckSeconds = 2f;

    private static readonly Dictionary<int, string> Names = new()
    {
        [FoodHealthSettings.WindChillEffect] = "Wind Chill",
        [FoodHealthSettings.FeverEffect] = "High Temperature",
    };

    private readonly TableKeeper _keeper = new();
    private readonly Dictionary<int, float> _reported = new();
    private readonly Action<string> _log;
    private IReadOnlyDictionary<int, TableChange> _changes = new Dictionary<int, TableChange>();
    private float _nextCheckAt;

    public SicknessChanceTable(Action<string> log)
    {
        _log = log;
    }

    /// <summary>Called on the main thread at start and after each change; the next check writes the new chances.</summary>
    public void Use(IReadOnlyDictionary<int, TableChange> changes)
    {
        _changes = changes;
        _nextCheckAt = 0f;
    }

    public void Tick(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        var rows = GameSingleton.Existing<ConfigManager>()?._Config_Effect_Dict;

        if (rows == null || rows.Count == 0) return;

        foreach (var effectId in FoodHealthSettings.SicknessEffects)
        {
            if (!rows.TryGetValue(effectId, out var row) || row == null) continue;

            _changes.TryGetValue(effectId, out var change);

            var key = FoodHealthSettings.EffectKey(effectId);

            if (_keeper.Target(key, row.Pointer.ToInt64(), row._FloatValue0_k__BackingField, change) is not { } chance) continue;

            if (row._FloatValue0_k__BackingField != chance) row._FloatValue0_k__BackingField = chance;

            Report(effectId, key, chance);
        }
    }

    /// <summary>Logged when a chance changes, not every check.</summary>
    private void Report(int effectId, string key, float chance)
    {
        if (_reported.TryGetValue(effectId, out var last) && last == chance) return;

        _reported[effectId] = chance;
        _log($"the chance of catching {Names[effectId]} in a cold wave is now {chance:0.###} (the game's own is {_keeper.GameValue(key):0.###})");
    }
}
