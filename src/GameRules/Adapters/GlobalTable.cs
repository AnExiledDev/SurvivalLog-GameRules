using System;
using System.Collections.Generic;
using System.Reflection;
using GameCore.HotUpdate;
using GameRules.Core;
using HarmonyLib;

namespace GameRules.Adapters;

/// <summary>
/// Writes the settings groups' changes into the game's GlobalSetting table. Every GameKey.GlobalSetting_X getter fills
/// the static field GameKey._GlobalSetting_X from the table entry (Params1 for an int getter, Params2 for a float one)
/// only while the field is 0, and trusts the field from then on, so both the entry and the field are written. Checked
/// every second: the table loads after the plugin, and a reload builds fresh entries holding the game's values.
/// </summary>
internal sealed class GlobalTable
{
    private const float CheckSeconds = 1f;

    private readonly TableChanges _changes;
    private readonly TableKeeper _keeper = new();
    private readonly Dictionary<string, Slot?> _slots = new();
    private readonly Dictionary<string, float> _reported = new();
    private readonly Action<string> _log;
    private float _nextCheckAt;

    public GlobalTable(TableChanges changes, Action<string> log)
    {
        _changes = changes;
        _log = log;
    }

    public void Tick(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        var table = GameSingleton.Existing<ConfigManager>()?._Config_GlobalSetting_Dict;

        if (table == null || table.Count == 0) return;

        var changes = _changes.ByKey();
        var keys = new HashSet<string>(_keeper.Keys);

        keys.UnionWith(changes.Keys);

        foreach (var key in keys)
        {
            if (SlotFor(key) is not { } slot || !table.TryGetValue(slot.TableKey, out var entry) || entry == null) continue;

            changes.TryGetValue(key, out var change);

            var entryValue = slot.IsInt ? entry._Params1_k__BackingField : entry._Params2_k__BackingField;

            if (_keeper.Target(key, entry.Pointer.ToInt64(), entryValue, change) is not { } target) continue;

            Write(slot, entry, target);
            Report(key, slot, target);
        }
    }

    private static void Write(Slot slot, Config_GlobalSetting entry, float target)
    {
        if (slot.IsInt)
        {
            var value = (int)Math.Round(target);

            if (entry._Params1_k__BackingField != value) entry._Params1_k__BackingField = value;
            if (!Equals(slot.Field.GetValue(null), value)) slot.Field.SetValue(null, value);

            return;
        }

        if (entry._Params2_k__BackingField != target) entry._Params2_k__BackingField = target;
        if (!Equals(slot.Field.GetValue(null), target)) slot.Field.SetValue(null, target);
    }

    /// <summary>Logged when a key's value changes, not every second.</summary>
    private void Report(string key, Slot slot, float target)
    {
        if (_reported.TryGetValue(key, out var last) && last == target) return;

        _reported[key] = target;

        var shown = slot.IsInt ? Math.Round(target).ToString() : target.ToString("0.####");

        _log($"{key} is now {shown} (the game's own is {_keeper.GameValue(key):0.####})");
    }

    /// <summary>The static field and table key behind a setting, looked up once. A key the game lacks is logged once and skipped.</summary>
    private Slot? SlotFor(string key)
    {
        if (_slots.TryGetValue(key, out var known)) return known;

        var field = AccessTools.Property(typeof(GameKey), "_GlobalSetting_" + key);
        var tableKey = AccessTools.Property(typeof(GameKey), "Key_GlobalSetting_" + key)?.GetValue(null) as string ?? key;
        Slot? slot = null;

        if (field != null && (field.PropertyType == typeof(int) || field.PropertyType == typeof(float)))
        {
            slot = new Slot(field, tableKey, field.PropertyType == typeof(int));
        }
        else
        {
            _log($"{HookFailure.Prefix} the game value {key}: the game has no such setting, so it keeps the game's rule");
        }

        _slots[key] = slot;

        return slot;
    }

    private sealed record Slot(PropertyInfo Field, string TableKey, bool IsInt);
}
