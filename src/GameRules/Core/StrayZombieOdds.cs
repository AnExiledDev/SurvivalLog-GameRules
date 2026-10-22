using System;
using System.Globalization;
using System.Linq;

namespace GameRules.Core;

/// <summary>The stray zombie rules the GlobalSetting table cannot carry: the leader chance curve and extra kill loot.</summary>
public static class StrayZombieOdds
{
    /// <summary>
    /// The game's leader chance curve ("day:chance;day:chance", such as "15:0;9999:0.3") with every chance multiplied
    /// and kept between 0 and 1. A curve that does not read as one is returned as it is, so the game's rule stays.
    /// </summary>
    public static string LeaderCurve(string curve, float multiplier)
    {
        if (multiplier == 1f || string.IsNullOrEmpty(curve)) return curve;

        var points = curve.Split(';').Select(point => point.Split(':')).ToList();

        if (points.Any(parts => parts.Length != 2 || !IsChance(parts[1]))) return curve;

        return string.Join(";", points.Select(parts => parts[0] + ":" + Scaled(parts[1], multiplier)));
    }

    /// <summary>
    /// How many times a killed stray zombie's loot is rolled: the whole part of the multiplier always, and one more
    /// when <paramref name="roll"/> (0 to 1) falls under the fraction. 1.5 gives one or two rolls, half the time each.
    /// </summary>
    public static int LootRolls(float multiplier, double roll)
    {
        if (multiplier <= 0f) return 0;

        var whole = (int)Math.Floor(multiplier);

        return roll < multiplier - whole ? whole + 1 : whole;
    }

    private static bool IsChance(string text) => float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out _);

    private static string Scaled(string chance, float multiplier)
    {
        var value = float.Parse(chance, NumberStyles.Float, CultureInfo.InvariantCulture) * multiplier;

        return Math.Clamp(value, 0f, 1f).ToString("0.####", CultureInfo.InvariantCulture);
    }
}
