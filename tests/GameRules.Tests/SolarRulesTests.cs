using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class SolarRulesTests
{
    private const float BasePower = 12f;

    private static SolarReading Sunny(float areaLight = 1f, float efficiency = 1f) =>
        new(Power: 2 * BasePower * areaLight * efficiency, BasePower, areaLight, efficiency, IsBadWeather: false);

    private static SolarReading Storm(float areaLight = 1f, float efficiency = 1f) =>
        new(Power: 0f, BasePower, areaLight, efficiency, IsBadWeather: true);

    [Fact]
    public void The_games_own_rules_change_nothing()
    {
        Assert.Equal(new SolarResult(24f, false), SolarRules.Apply(Sunny(), RuleSettings.Game));
        Assert.Equal(new SolarResult(0f, false), SolarRules.Apply(Storm(), RuleSettings.Game));
    }

    [Fact]
    public void The_multiplier_scales_every_panels_output()
    {
        var settings = RuleSettings.Game with { SolarMultiplier = 1.5f };

        Assert.Equal(36f, SolarRules.Apply(Sunny(), settings).Power);
    }

    [Fact]
    public void Bad_weather_gives_the_chosen_share_of_a_cloudy_day()
    {
        var settings = RuleSettings.Game with { BadWeatherSolar = 0.5f };

        var result = SolarRules.Apply(Storm(areaLight: 0.8f, efficiency: 1.4f), settings);

        Assert.Equal(0.5f * BasePower * 0.8f * 1.4f, result.Power, precision: 4);
        Assert.True(result.ClearsBadWeather);
    }

    [Fact]
    public void Bad_weather_output_is_scaled_by_the_multiplier_too()
    {
        var settings = RuleSettings.Game with { BadWeatherSolar = 1f, SolarMultiplier = 2f };

        Assert.Equal(2f * BasePower, SolarRules.Apply(Storm(), settings).Power);
    }

    [Fact]
    public void A_panel_no_light_reaches_stays_dark_in_bad_weather()
    {
        var settings = RuleSettings.Game with { BadWeatherSolar = 1f };

        Assert.Equal(new SolarResult(0f, false), SolarRules.Apply(Storm(areaLight: 0f), settings));
    }

    /// <summary>The panel hook skips its reads whenever the settings change no solar output; that skip is only safe while this holds.</summary>
    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    public void Settings_that_change_no_solar_output_leave_every_panel_as_the_game_has_it(float badWeatherSolar)
    {
        var settings = RuleSettings.Game with { BadWeatherSolar = badWeatherSolar };

        Assert.False(settings.ChangesSolar);
        Assert.Equal(new SolarResult(24f, false), SolarRules.Apply(Sunny(), settings));
        Assert.Equal(new SolarResult(0f, false), SolarRules.Apply(Storm(), settings));
        Assert.Equal(new SolarResult(7f, false), SolarRules.Apply(new SolarReading(7f, BasePower, 0.5f, 1.2f, IsBadWeather: true), settings));
    }

    [Theory]
    [InlineData("bad_weather", true)]
    [InlineData("bad_weathe", false)]
    [InlineData("bad_weather_", false)]
    [InlineData("Bad_weather", false)]
    [InlineData("night", false)]
    [InlineData("", false)]
    public void Only_the_games_exact_bad_weather_reason_counts_as_bad_weather(string reason, bool isBadWeather)
    {
        Assert.Equal(isBadWeather, SolarRules.IsBadWeatherReason(reason.AsSpan()));
    }

    [Fact]
    public void Good_weather_output_is_never_replaced_by_the_bad_weather_share()
    {
        var settings = RuleSettings.Game with { BadWeatherSolar = 1f };

        Assert.Equal(new SolarResult(24f, false), SolarRules.Apply(Sunny(), settings));
    }
}
