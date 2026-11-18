using System;
using System.Collections.Generic;
using ExiledToolkit;
using ExiledToolkit.Game;
using GameCore.HotUpdate;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Writes the chance of catching Wind Chill or a High Temperature in a cold wave without heat. Each is FloatValue0 of
/// its row in the game's effect table (Config_Effect), read every time the game rolls for it, so a changed row takes
/// effect at the next roll. The game builds the table once, so one pass writes the chances; it is tried every two
/// seconds until the table has loaded, and runs again when a setting changes or a save loads. The keeper remembers the game's chance, so a setting put back to 1 restores it.
/// </summary>
internal sealed class SicknessChanceTable
{
    private static readonly Dictionary<int, string> Names = new()
    {
        [FoodHealthSettings.WindChillEffect] = "Wind Chill",
        [FoodHealthSettings.FeverEffect] = "High Temperature",
    };

    private readonly TableKeeper _keeper = new();
    private readonly TablePass _pass = new(retrySeconds: 2f);
    private readonly Dictionary<int, float> _reported = new();
    private readonly Action<string> _log;
    private IReadOnlyDictionary<int, TableChange> _changes = new Dictionary<int, TableChange>();

    public SicknessChanceTable(Action<string> log)
    {
        _log = log;
    }

    /// <summary>Called on the main thread at start and after each change; the next check writes the new chances.</summary>
    public void Use(IReadOnlyDictionary<int, TableChange> changes)
    {
        _changes = changes;
        _pass.Redo();
    }

    /// <summary>A save loaded: one more pass, in case anything put the game's values back.</summary>
    public void Redo() => _pass.Redo();

    public void Tick(float now)
    {
        if (!_pass.IsDue(now)) return;

        // At the game's chances, with none ever changed, there is nothing to write or put back, so the game is not read.
        if (_changes.Count == 0 && _keeper.IsEmpty)
        {
            _pass.Done();

            return;
        }

        var rows = GameSingleton.Existing<ConfigManager>()?._Config_Effect_Dict;

        if (rows == null || rows.Count == 0) return;

        foreach (var effectId in FoodHealthSettings.SicknessEffects)
        {
            if (!rows.TryGetValue(effectId, out var row) || row == null) continue;

            _changes.TryGetValue(effectId, out var change);

            var key = FoodHealthSettings.EffectKey(effectId);

            if (_keeper.Target(key, row.Pointer.ToInt64(), row._FloatValue0_k__BackingField, change) is not { } chance) continue;

            if (row._FloatValue0_k__BackingField != chance) row._FloatValue0_k__BackingField = chance;

            ReportRow(effectId, chance, _keeper.GameValue(key));
            Report(effectId, key, chance);
        }

        _pass.Done();
    }

    /// <summary>Tells Mod Manager the row holds the mod's chance, or that it was put back to the game's.</summary>
    private static void ReportRow(int effectId, float chance, float? gameChance)
    {
        var field = nameof(Config_Effect.FloatValue0);

        if (chance == gameChance) Claims.PutBack(Plugin.Guid, nameof(Config_Effect), effectId, field);
        else Claims.Wrote(Plugin.Guid, nameof(Config_Effect), effectId, field, chance);
    }

    /// <summary>Logged when a chance changes, not at every pass.</summary>
    private void Report(int effectId, string key, float chance)
    {
        if (_reported.TryGetValue(effectId, out var last) && last == chance) return;

        _reported[effectId] = chance;
        _log($"the chance of catching {Names[effectId]} in a cold wave is now {chance:0.###} (the game's is {_keeper.GameValue(key):0.###})");
    }
}
