using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRules.Core;

/// <summary>One hour of the game's day-by-day weather table.</summary>
public readonly record struct WeatherHour(int Day, int Hour, int Weather);

public readonly record struct WeatherChance(int Weather, float Chance);

/// <summary>The weather the player wants instead of the game's table. Chances are shares, so they need not add up to 100.</summary>
public sealed record WeatherSettings(bool RandomWeather, int Seed, IReadOnlyList<WeatherChance> Chances)
{
    public const int Clear = 1;
    public const int Cloudy = 2;
    public const int Rain = 3;
    public const int HeavyRain = 10;
    public const int ContinuousRain = 11;
    public const int Smog = 12;

    /// <summary>The share of each weather in the game's own table once its cold waves are set aside, rounded to whole percents.</summary>
    public static IReadOnlyList<WeatherChance> GameMix { get; } = new[]
    {
        new WeatherChance(Clear, 34f), new WeatherChance(Cloudy, 28f), new WeatherChance(Rain, 20f),
        new WeatherChance(HeavyRain, 4f), new WeatherChance(ContinuousRain, 9f), new WeatherChance(Smog, 5f),
    };

    public static WeatherSettings Game { get; } = new(RandomWeather: false, Seed: 1, GameMix);

    public bool IsActive => RandomWeather && Chances.Any(chance => chance.Chance > 0f);
}

/// <summary>
/// Rolls new weather over the game's table. Each run of one weather within a day becomes one rolled weather, so a
/// rainy afternoon becomes, say, a cloudy afternoon rather than a flicker of hourly changes. The roll depends only on
/// the seed, the day and the hour the run starts, so the forecast, a reload and a restart all agree.
/// </summary>
public static class WeatherShuffle
{
    /// <summary>The hours whose weather changes. Kept weather (cold waves) and anchor days stay as the game has them.</summary>
    public static IReadOnlyList<WeatherHour> Plan(IEnumerable<WeatherHour> table, WeatherSettings settings, Func<int, bool> isKeptWeather, Func<int, bool> isAnchorDay)
    {
        if (!settings.IsActive) return Array.Empty<WeatherHour>();

        var changes = new List<WeatherHour>();

        foreach (var run in Runs(table))
        {
            var first = run[0];

            if (isKeptWeather(first.Weather) || isAnchorDay(first.Day)) continue;

            var rolled = Pick(settings, Roll(settings.Seed, first.Day, first.Hour));

            if (rolled == first.Weather) continue;

            changes.AddRange(run.Select(hour => hour with { Weather = rolled }));
        }

        return changes;
    }

    /// <summary>Consecutive hours of one day that share a weather.</summary>
    private static IEnumerable<List<WeatherHour>> Runs(IEnumerable<WeatherHour> table)
    {
        var run = new List<WeatherHour>();

        foreach (var hour in table.OrderBy(hour => hour.Day).ThenBy(hour => hour.Hour))
        {
            var continuesRun = run.Count > 0 && run[^1].Day == hour.Day && run[^1].Weather == hour.Weather;

            if (!continuesRun && run.Count > 0)
            {
                yield return run;

                run = new List<WeatherHour>();
            }

            run.Add(hour);
        }

        if (run.Count > 0) yield return run;
    }

    private static int Pick(WeatherSettings settings, double roll)
    {
        var shares = settings.Chances.Where(chance => chance.Chance > 0f).ToList();
        var point = roll * shares.Sum(chance => (double)chance.Chance);

        foreach (var share in shares)
        {
            if (point < share.Chance) return share.Weather;

            point -= share.Chance;
        }

        return shares[^1].Weather;
    }

    /// <summary>A number from 0 up to 1, fixed for a seed, day and hour. A mixing hash, since .NET's own hashes change between runs.</summary>
    private static double Roll(int seed, int day, int hour)
    {
        var mixed = unchecked((ulong)(uint)seed * 0x9E3779B97F4A7C15UL ^ (ulong)(uint)day * 0xC2B2AE3D27D4EB4FUL ^ (ulong)(uint)hour * 0x165667B19E3779F9UL);

        mixed = unchecked((mixed ^ (mixed >> 30)) * 0xBF58476D1CE4E5B9UL);
        mixed = unchecked((mixed ^ (mixed >> 27)) * 0x94D049BB133111EBUL);
        mixed ^= mixed >> 31;

        return (mixed >> 11) * (1.0 / (1UL << 53));
    }
}
