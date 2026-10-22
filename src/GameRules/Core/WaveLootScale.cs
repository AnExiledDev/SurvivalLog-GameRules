using System;
using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>
/// Scales a whole-number GlobalSetting, rounded half up and never below 1: the game falls back to its built-in value
/// for a 0, and the table writer would otherwise round a half to even.
/// </summary>
public sealed record WholeScaleBy(string Key, float Factor) : TableChange(Key)
{
    public override float Apply(float gameValue) => Math.Max(1f, (float)Math.Round(gameValue * (double)Factor, MidpointRounding.AwayFromZero));
}

/// <summary>
/// The loot a zombie horde wave leaves. The game gives a wave CatBase kinds of item plus one per CatPerKill kills, up
/// to CatMax, and one of each plus one per QtyPerKill kills, up to QtyMax. More loot per kill means fewer kills per
/// step and higher limits.
/// </summary>
public static class WaveLootScale
{
    public static IEnumerable<TableChange> Changes(float multiplier)
    {
        if (multiplier == 1f || multiplier <= 0f) yield break;

        yield return new WholeScaleBy("TowerWaveLootCatPerKill", 1f / multiplier);
        yield return new WholeScaleBy("TowerWaveLootQtyPerKill", 1f / multiplier);
        yield return new WholeScaleBy("TowerWaveLootCatMax", multiplier);
        yield return new WholeScaleBy("TowerWaveLootQtyMax", multiplier);
    }
}
