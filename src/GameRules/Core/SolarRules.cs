using System;

namespace GameRules.Core;

/// <summary>
/// One solar panel's output as the game worked it out: Power = LightValue x BasePower x AreaLightMul x Efficiency,
/// by day only. LightValue is the weather's light (2 sunny, 1 cloudy or rain, 0 in storms and heavy rain).
/// </summary>
public readonly record struct SolarReading(float Power, float BasePower, float AreaLightMul, float Efficiency, bool IsBadWeather);

/// <summary>The output to report instead. <see cref="ClearsBadWeather"/> when the panel now runs despite the weather.</summary>
public readonly record struct SolarResult(float Power, bool ClearsBadWeather);

public static class SolarRules
{
    /// <summary>The game's reason text for a panel stopped by the weather, compared in place without copying it out.</summary>
    public static bool IsBadWeatherReason(ReadOnlySpan<char> reason) => reason.SequenceEqual("bad_weather".AsSpan());

    /// <summary>
    /// Bad weather: the game gives nothing, so the panel gets <see cref="RuleSettings.BadWeatherSolar"/> of a cloudy
    /// day's light; a panel standing where no light reaches (AreaLightMul 0) still makes nothing. Then every output is
    /// scaled.
    /// </summary>
    public static SolarResult Apply(SolarReading reading, RuleSettings settings)
    {
        var power = reading.Power;
        var clearsBadWeather = false;

        if (reading.IsBadWeather && settings.BadWeatherSolar > 0f)
        {
            power = settings.BadWeatherSolar * reading.BasePower * reading.AreaLightMul * reading.Efficiency;
            clearsBadWeather = power > 0f;
        }

        return new SolarResult(power * settings.SolarMultiplier, clearsBadWeather);
    }
}
