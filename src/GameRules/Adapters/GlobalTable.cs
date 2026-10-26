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
/// only while the field is 0, and trusts the field from then on, so both the entry and the field are written. An int
/// entry whose Params2 matches its Params1 gets Params2 written too, for game code that reads the key by name. Checked
/// every second: the table loads after the plugin, and a reload builds fresh entries holding the game's values.
/// </summary>
internal sealed class GlobalTable
{
    private const float CheckSeconds = 1f;

    private readonly TableKeeper _keeper = new();
    private readonly TableKeys _keys;
    private readonly Dictionary<string, Slot?> _slots = new();
    private readonly Dictionary<string, float> _reported = new();
    private readonly Action<string> _log;
    private float _nextCheckAt;

    public GlobalTable(TableChanges changes, Action<string> log)
    {
        _keys = new TableKeys(changes, _keeper);
        _log = log;
    }

    public void Tick(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        // The key list is only rebuilt when a setting changes; with nothing to keep, the game is not read at all.
        var keys = _keys.Keys;

        if (keys.Count == 0) return;

        var table = GameSingleton.Existing<ConfigManager>()?._Config_GlobalSetting_Dict;

        if (table == null || table.Count == 0) return;

        var changes = _keys.ByKey;

        for (var i = 0; i < keys.Count; i++)
        {
            var key = keys[i];

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

            if (entry._Params1_k__BackingField != value && IntEntry.KeepsParams2InStep(entry._Params1_k__BackingField, entry._Params2_k__BackingField)) entry._Params2_k__BackingField = value;
            if (entry._Params1_k__BackingField != value) entry._Params1_k__BackingField = value;
            if (slot.GetInt!() != value) slot.SetInt!(value);

            return;
        }

        if (entry._Params2_k__BackingField != target) entry._Params2_k__BackingField = target;
        if (!slot.GetFloat!().Equals(target)) slot.SetFloat!(target);
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
        var slot = field == null ? null : Slot.For(field, tableKey);

        if (slot == null) _log($"{HookFailure.Prefix} the game value {key}: the game has no such setting, so it keeps the game's rule");

        _slots[key] = slot;

        return slot;
    }

    /// <summary>
    /// The static field is read and written through typed delegates made once: through reflection every read and write
    /// boxes the value, for every key, every second.
    /// </summary>
    private sealed record Slot(string TableKey, bool IsInt, Func<int>? GetInt, Action<int>? SetInt, Func<float>? GetFloat, Action<float>? SetFloat)
    {
        /// <summary>Null for a field that is not a static int or float with both a getter and a setter.</summary>
        public static Slot? For(PropertyInfo field, string tableKey)
        {
            var getter = field.GetGetMethod(nonPublic: true);
            var setter = field.GetSetMethod(nonPublic: true);

            if (getter is not { IsStatic: true } || setter is not { IsStatic: true }) return null;

            if (field.PropertyType == typeof(int))
            {
                return new Slot(tableKey, IsInt: true,
                    (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), getter), (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), setter), null, null);
            }

            if (field.PropertyType == typeof(float))
            {
                return new Slot(tableKey, IsInt: false,
                    null, null, (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), getter), (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), setter));
            }

            return null;
        }
    }
}
