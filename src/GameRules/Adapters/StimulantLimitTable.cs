using System;
using System.Collections.Generic;
using ExiledToolkit.Game;
using GameCore.HotUpdate;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Writes the daily limit on each stimulant: DailyMax of its row in the game's item table (Config_Item). The game
/// checks it every time an item is used or a queued use starts (ItemManager.IsDailyMaxReached), counts uses only while
/// it is above 0, and clears the count each new day. Checked every two seconds: the table loads after the plugin, and
/// a reload builds fresh rows holding the game's values. The keeper remembers the game's 0, so turning the limit off
/// puts it back.
/// </summary>
internal sealed class StimulantLimitTable
{
    private const float CheckSeconds = 2f;

    private readonly TableKeeper _keeper = new();
    private readonly Action<string> _log;
    private IReadOnlyDictionary<int, TableChange> _changes = new Dictionary<int, TableChange>();
    private int _limit;
    private int? _reported;
    private float _nextCheckAt;

    public StimulantLimitTable(Action<string> log)
    {
        _log = log;
    }

    /// <summary>Called on the main thread at start and after each change; the next check writes the new limit.</summary>
    public void Use(BodySettings settings)
    {
        _limit = Math.Max(0, settings.StimulantDailyLimit);
        _changes = settings.DailyLimitChanges();
        _nextCheckAt = 0f;
    }

    public void Tick(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        // With no limit, and none ever set, there is nothing to write or put back, so the game is not read.
        if (_changes.Count == 0 && _keeper.IsEmpty) return;

        var rows = GameSingleton.Existing<ConfigManager>()?._Config_Item_Dict;

        if (rows == null || rows.Count == 0) return;

        var written = 0;

        foreach (var item in Stimulants.Items)
        {
            if (!rows.TryGetValue(item, out var row) || row == null) continue;

            _changes.TryGetValue(item, out var change);

            if (_keeper.Target(BodySettings.DailyLimitKey(item), row.Pointer.ToInt64(), row._DailyMax_k__BackingField, change) is not { } target) continue;

            var limit = (int)Math.Round(target);

            if (row._DailyMax_k__BackingField != limit) row._DailyMax_k__BackingField = limit;

            written++;
        }

        if (written > 0) Report();
    }

    /// <summary>Logged when the limit changes, not every check.</summary>
    private void Report()
    {
        if (_reported == _limit) return;

        _reported = _limit;
        _log(_limit == 0 ? "stimulants have no daily limit" : $"each kind of stimulant can be used {_limit} time(s) a day");
    }
}
