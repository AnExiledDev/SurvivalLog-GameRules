using System;
using System.Collections.Generic;
using ExiledToolkit;
using ExiledToolkit.Game;
using GameCore.HotUpdate;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Writes the daily limit on each stimulant: DailyMax of its row in the game's item table (Config_Item). The game
/// checks it every time an item is used or a queued use starts (ItemManager.IsDailyMaxReached), counts uses only while
/// it is above 0, and clears the count each new day. The game builds the table once, so one pass writes the limits; it
/// is tried every two seconds until the table has loaded, and runs again when a setting changes or a save loads. The keeper remembers the game's 0, so turning the limit off
/// puts it back.
/// </summary>
internal sealed class StimulantLimitTable
{
    private readonly TableKeeper _keeper = new();
    private readonly TablePass _pass = new(retrySeconds: 2f);
    private readonly Action<string> _log;
    private IReadOnlyDictionary<int, TableChange> _changes = new Dictionary<int, TableChange>();
    private int _limit;
    private int? _reported;

    public StimulantLimitTable(Action<string> log)
    {
        _log = log;
    }

    /// <summary>Called on the main thread at start and after each change; the next check writes the new limit.</summary>
    public void Use(BodySettings settings)
    {
        _limit = Math.Max(0, settings.StimulantDailyLimit);
        _changes = settings.DailyLimitChanges();
        _pass.Redo();
    }

    /// <summary>A save loaded: one more pass, in case anything put the game's values back.</summary>
    public void Redo() => _pass.Redo();

    public void Tick(float now)
    {
        if (!_pass.IsDue(now)) return;

        // With no limit, and none ever set, there is nothing to write or put back, so the game is not read.
        if (_changes.Count == 0 && _keeper.IsEmpty)
        {
            _pass.Done();

            return;
        }

        var rows = GameSingleton.Existing<ConfigManager>()?._Config_Item_Dict;

        if (rows == null || rows.Count == 0) return;

        var written = 0;

        foreach (var item in Stimulants.Items)
        {
            if (!rows.TryGetValue(item, out var row) || row == null) continue;

            _changes.TryGetValue(item, out var change);

            var key = BodySettings.DailyLimitKey(item);

            if (_keeper.Target(key, row.Pointer.ToInt64(), row._DailyMax_k__BackingField, change) is not { } target) continue;

            var limit = (int)Math.Round(target);

            if (row._DailyMax_k__BackingField != limit) row._DailyMax_k__BackingField = limit;

            ReportRow(item, limit, _keeper.GameValue(key));

            written++;
        }

        _pass.Done();

        if (written > 0) Report();
    }

    /// <summary>Tells Mod Manager the row holds the mod's limit, or that it was put back to the game's.</summary>
    private static void ReportRow(int item, int limit, float? gameLimit)
    {
        var field = nameof(Config_Item.DailyMax);

        if (gameLimit is { } game && limit == (int)Math.Round(game)) Claims.PutBack(Plugin.Guid, nameof(Config_Item), item, field);
        else Claims.Wrote(Plugin.Guid, nameof(Config_Item), item, field, limit);
    }

    /// <summary>Logged when the limit changes, not at every pass.</summary>
    private void Report()
    {
        if (_reported == _limit) return;

        _reported = _limit;
        _log(_limit == 0 ? "stimulants have no daily limit" : $"each kind of stimulant can be used {_limit} time(s) a day");
    }
}
