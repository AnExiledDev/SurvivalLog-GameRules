using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Scales battery capacity in the game's electrical table (Config_FurnitureElectrical). The power manager adds up
/// the Capacity of every placed battery-type device to get the base's storage, so a bigger number there is a bigger
/// battery everywhere the game shows or uses it. Each row is scaled once: a row already written is remembered, and a
/// reload builds fresh rows with the game's numbers, which are then scaled in turn.
/// </summary>
internal sealed class BatteryTable
{
    private const float CheckSeconds = 2f;

    private readonly float _multiplier;
    private readonly Action<string> _log;
    private readonly HashSet<IntPtr> _scaledRows = new();
    private float _nextCheckAt;

    public BatteryTable(float multiplier, Action<string> log)
    {
        _multiplier = multiplier;
        _log = log;
    }

    public void Tick(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        var rows = GameSingleton.Existing<ConfigManager>()?._Config_FurnitureElectrical_Dict;

        if (rows == null || rows.Count == 0) return;

        var scaled = 0;

        foreach (var row in rows.Values)
        {
            if (row == null || !_scaledRows.Add(row.Pointer)) continue;

            var gameCapacity = row._Capacity_k__BackingField;
            var capacity = BatteryCapacity.Scale(row._ElectricalType_k__BackingField, gameCapacity, _multiplier);

            if (capacity == gameCapacity) continue;

            row._Capacity_k__BackingField = capacity;
            scaled++;
        }

        if (scaled > 0) _log($"{scaled} battery types now hold {_multiplier:0.##}x the game's power");
    }
}
