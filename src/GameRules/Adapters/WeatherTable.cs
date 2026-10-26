using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Rewrites the game's day-by-day weather table (Config_DailyWeather, days 1-100, one row per hour). The game reads
/// each hour's weather from it when the hour comes, and tomorrow's forecast from it too, then applies the difficulty's
/// own weather changes on top, so rewriting the table changes the weather, the forecast and nothing else. Endless mode
/// rolls its weather elsewhere and is not affected. The table is only there once the game has loaded its configs, and
/// a reload builds fresh rows, so the first row is checked every few seconds and a new one means the table is
/// rewritten again.
/// </summary>
internal sealed class WeatherTable
{
    private const float CheckSeconds = 2f;

    private readonly WeatherSettings _settings;
    private readonly Action<string> _log;
    private float _nextCheckAt;
    private IntPtr _rewrittenFirstRow;

    public WeatherTable(WeatherSettings settings, Action<string> log)
    {
        _settings = settings;
        _log = log;
    }

    public void Tick(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        var config = GameSingleton.Existing<ConfigManager>();
        var rows = config?._Config_DailyWeather_Dict;

        if (config == null || rows == null || rows.Count == 0) return;

        var firstRow = config.Get_Config_DailyWeather_ByDayHour(1, 0);

        if (firstRow == null || firstRow.Pointer == _rewrittenFirstRow) return;

        _rewrittenFirstRow = firstRow.Pointer;
        Rewrite(config, rows);
    }

    private void Rewrite(ConfigManager config, Il2CppSystem.Collections.Generic.Dictionary<int, Config_DailyWeather> rows)
    {
        var anchorDays = DifficultyTools.WeatherAnchorDays;

        if (anchorDays == null)
        {
            _log("could not read the game's fixed-weather days, so the weather stays as in the game");

            return;
        }

        var byHour = new Dictionary<(int Day, int Hour), Config_DailyWeather>();

        foreach (var row in rows.Values)
        {
            if (row != null) byHour[(row._Day_k__BackingField, row._Hour_k__BackingField)] = row;
        }

        // Each weather's config row is looked up once: the table holds a handful of weathers over a hundred days.
        var coldWaves = new Dictionary<int, bool>();

        bool IsKeptWeather(int weather)
        {
            if (!coldWaves.TryGetValue(weather, out var isColdWave)) coldWaves[weather] = isColdWave = IsColdWave(config, weather);

            return isColdWave;
        }

        var table = byHour.Select(pair => new WeatherHour(pair.Key.Day, pair.Key.Hour, pair.Value._WeatherID_k__BackingField));
        var changes = WeatherShuffle.Plan(table, _settings, IsKeptWeather, anchorDays.Contains);

        foreach (var change in changes) byHour[(change.Day, change.Hour)]._WeatherID_k__BackingField = change.Weather;

        var mix = string.Join(", ", byHour.Values.GroupBy(row => row._WeatherID_k__BackingField).OrderBy(group => group.Key).Select(group => $"{group.Key}: {group.Count()}h"));

        _log($"weather rolled with seed {_settings.Seed}: {changes.Count} of {byHour.Count} hours changed; cold waves and fixed-weather days kept. Hours per weather id now {mix}");
    }

    /// <summary>Cold waves belong to the game's cold-wave events, so they stay where the game put them.</summary>
    private static bool IsColdWave(ConfigManager config, int weatherId) => config.Get_Config_Weather(weatherId) is { } weather && weather._ColdValue_k__BackingField > 0;
}
