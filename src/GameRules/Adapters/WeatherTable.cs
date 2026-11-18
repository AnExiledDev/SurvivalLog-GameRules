using System;
using System.Collections.Generic;
using System.Linq;
using ExiledToolkit;
using ExiledToolkit.Game;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Rewrites the game's day-by-day weather table (Config_DailyWeather, days 1-100, one row per hour). The game reads
/// each hour's weather from it when the hour comes, and tomorrow's forecast from it too, then applies the difficulty's
/// own weather changes on top, so rewriting the table changes the weather, the forecast and nothing else. Endless mode
/// rolls its weather elsewhere and is not affected. The game builds the table once, when it starts, so it is rewritten
/// once: tried every two seconds until the table has loaded, then again only after a save loads, and only if the table
/// holds new rows by then (rolling the same rows twice would change the weather). Each hour it changes is reported
/// to Mod Manager, and a table built again holds the game's weather, so the hours reported for the old one are taken
/// back first.
/// </summary>
internal sealed class WeatherTable
{
    private readonly WeatherSettings _settings;
    private readonly Action<string> _log;
    private readonly TablePass _pass = new(retrySeconds: 2f);
    private readonly List<int> _reported = new();
    private IntPtr _rewrittenFirstRow;

    public WeatherTable(WeatherSettings settings, Action<string> log)
    {
        _settings = settings;
        _log = log;
    }

    /// <summary>A save loaded: one more look, in case the game built the table again.</summary>
    public void Redo() => _pass.Redo();

    public void Tick(float now)
    {
        if (!_pass.IsDue(now)) return;

        var config = GameSingleton.Existing<ConfigManager>();
        var rows = config?._Config_DailyWeather_Dict;

        if (config == null || rows == null || rows.Count == 0) return;

        var firstRow = config.Get_Config_DailyWeather_ByDayHour(1, 0);

        if (firstRow == null) return;

        _pass.Done();

        if (firstRow.Pointer == _rewrittenFirstRow) return;

        _rewrittenFirstRow = firstRow.Pointer;
        TakeBack();
        Rewrite(config, rows);
    }

    private void Rewrite(ConfigManager config, Il2CppSystem.Collections.Generic.Dictionary<int, Config_DailyWeather> rows)
    {
        var anchorDays = DifficultyTools.WeatherAnchorDays;

        if (anchorDays == null)
        {
            _log("could not read the game's fixed-weather days, so the weather is left as it is");

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

        foreach (var change in changes)
        {
            var row = byHour[(change.Day, change.Hour)];

            row._WeatherID_k__BackingField = change.Weather;
            Report(row);
        }

        var mix = string.Join(", ", byHour.Values.GroupBy(row => row._WeatherID_k__BackingField).OrderBy(group => group.Key).Select(group => $"{group.Key}: {group.Count()}h"));

        _log($"weather rolled with seed {_settings.Seed}: {changes.Count} of {byHour.Count} hours changed; cold waves and fixed-weather days kept. Hours per weather id now {mix}");
    }

    private void Report(Config_DailyWeather row)
    {
        Claims.Wrote(Plugin.Guid, nameof(Config_DailyWeather), row._ID_k__BackingField, nameof(Config_DailyWeather.WeatherID), row._WeatherID_k__BackingField);
        _reported.Add(row._ID_k__BackingField);
    }

    private void TakeBack()
    {
        foreach (var id in _reported) Claims.PutBack(Plugin.Guid, nameof(Config_DailyWeather), id, nameof(Config_DailyWeather.WeatherID));

        _reported.Clear();
    }

    /// <summary>Cold waves belong to the game's cold-wave events, so they stay where the game put them.</summary>
    private static bool IsColdWave(ConfigManager config, int weatherId) => config.Get_Config_Weather(weatherId) is { } weather && weather._ColdValue_k__BackingField > 0;
}
