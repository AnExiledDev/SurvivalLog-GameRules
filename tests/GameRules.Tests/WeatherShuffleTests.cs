using System.Collections.Generic;
using System.Linq;
using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class WeatherShuffleTests
{
    private const int Cold = 5;

    private static readonly WeatherSettings OnlyClear = new(RandomWeather: true, Seed: 1, new[] { new WeatherChance(WeatherSettings.Clear, 1f) });

    /// <summary>A hundred days, each a rainy morning (0-11) and a cloudy evening (12-23).</summary>
    private static List<WeatherHour> RainyMornings(int days = 100) =>
        Enumerable.Range(1, days)
            .SelectMany(day => Enumerable.Range(0, 24).Select(hour => new WeatherHour(day, hour, hour < 12 ? WeatherSettings.Rain : WeatherSettings.Cloudy)))
            .ToList();

    private static Dictionary<(int Day, int Hour), int> Apply(List<WeatherHour> table, IReadOnlyList<WeatherHour> changes)
    {
        var result = table.ToDictionary(hour => (hour.Day, hour.Hour), hour => hour.Weather);

        foreach (var change in changes) result[(change.Day, change.Hour)] = change.Weather;

        return result;
    }

    private static IReadOnlyList<WeatherHour> Plan(List<WeatherHour> table, WeatherSettings settings) =>
        WeatherShuffle.Plan(table, settings, weather => weather == Cold, _ => false);

    [Fact]
    public void Off_leaves_the_games_weather_alone()
    {
        Assert.Empty(Plan(RainyMornings(), OnlyClear with { RandomWeather = false }));
    }

    [Fact]
    public void All_chances_at_zero_leaves_the_games_weather_alone()
    {
        Assert.Empty(Plan(RainyMornings(), OnlyClear with { Chances = new[] { new WeatherChance(WeatherSettings.Clear, 0f) } }));
    }

    [Fact]
    public void A_weather_with_the_only_chance_replaces_everything_else()
    {
        var table = RainyMornings();
        var weather = Apply(table, Plan(table, OnlyClear));

        Assert.All(weather.Values, value => Assert.Equal(WeatherSettings.Clear, value));
    }

    [Fact]
    public void A_weather_with_zero_chance_never_appears()
    {
        var settings = new WeatherSettings(true, 7, new[] { new WeatherChance(WeatherSettings.Clear, 50f), new WeatherChance(WeatherSettings.Rain, 0f), new WeatherChance(WeatherSettings.Smog, 50f) });
        var table = RainyMornings();
        var weather = Apply(table, Plan(table, settings));

        Assert.DoesNotContain(WeatherSettings.Rain, weather.Values);
        Assert.DoesNotContain(WeatherSettings.Cloudy, weather.Values);
    }

    [Fact]
    public void Each_run_of_one_weather_becomes_one_weather()
    {
        var table = RainyMornings();
        var settings = new WeatherSettings(true, 5, new[] { new WeatherChance(WeatherSettings.Clear, 50f), new WeatherChance(WeatherSettings.Smog, 50f) });
        var weather = Apply(table, Plan(table, settings));

        foreach (var day in Enumerable.Range(1, 100))
        {
            Assert.Single(Enumerable.Range(0, 12).Select(hour => weather[(day, hour)]).Distinct());
            Assert.Single(Enumerable.Range(12, 12).Select(hour => weather[(day, hour)]).Distinct());
        }

        Assert.Contains(Enumerable.Range(1, 100), day => weather[(day, 0)] != weather[(day, 12)]);
    }

    [Fact]
    public void The_same_seed_rolls_the_same_weather_and_another_seed_rolls_different_weather()
    {
        var table = RainyMornings();
        var settings = WeatherSettings.Game with { RandomWeather = true };
        var first = Apply(table, Plan(table, settings));
        var again = Apply(table, Plan(table, settings));
        var otherSeed = Apply(table, Plan(table, settings with { Seed = 2 }));

        Assert.Equal(first, again);
        Assert.NotEqual(first, otherSeed);
    }

    [Fact]
    public void Cold_waves_and_anchor_days_keep_the_games_weather()
    {
        var table = RainyMornings(10).Select(hour => hour.Day == 3 ? hour with { Weather = Cold } : hour).ToList();
        var changes = WeatherShuffle.Plan(table, OnlyClear, weather => weather == Cold, day => day == 6);

        Assert.DoesNotContain(changes, change => change.Day == 3);
        Assert.DoesNotContain(changes, change => change.Day == 6);
        Assert.Contains(changes, change => change.Day == 5);
    }

    [Fact]
    public void Over_many_days_each_weather_comes_up_about_as_often_as_its_chance()
    {
        var settings = new WeatherSettings(true, 3, new[] { new WeatherChance(WeatherSettings.Clear, 75f), new WeatherChance(WeatherSettings.Smog, 25f) });
        var table = RainyMornings(2000);
        var weather = Apply(table, Plan(table, settings));
        var clearShare = weather.Values.Count(value => value == WeatherSettings.Clear) / (double)weather.Count;

        Assert.InRange(clearShare, 0.72, 0.78);
    }
}
