using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class ColdWaveScaleTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void The_games_strength_keeps_the_games_cold(int gameCold)
    {
        Assert.Equal(gameCold, ColdWaveScale.Cold(gameCold, 1f));
    }

    [Fact]
    public void Zero_strength_makes_a_cold_wave_as_mild_as_any_day()
    {
        Assert.Equal(0, ColdWaveScale.Cold(3, 0f));
    }

    [Fact]
    public void Double_strength_doubles_each_step_of_cold()
    {
        Assert.Equal(2, ColdWaveScale.Cold(1, 2f));
        Assert.Equal(6, ColdWaveScale.Cold(3, 2f));
    }

    [Fact]
    public void A_half_step_of_cold_rounds_up_so_a_light_cold_wave_stays_cold()
    {
        Assert.Equal(1, ColdWaveScale.Cold(1, 0.5f));
        Assert.Equal(2, ColdWaveScale.Cold(3, 0.5f));
    }

    [Fact]
    public void A_mild_day_stays_mild_at_any_strength()
    {
        Assert.Equal(0, ColdWaveScale.Cold(0, 3f));
    }

    [Fact]
    public void Bonus_levels_add_to_the_resistance_from_talents()
    {
        Assert.Equal(1f, ColdWaveScale.ResistTiers(0f, 1));
        Assert.Equal(3f, ColdWaveScale.ResistTiers(1f, 2));
    }

    [Fact]
    public void No_bonus_keeps_the_games_resistance()
    {
        Assert.Equal(1f, ColdWaveScale.ResistTiers(1f, 0));
    }
}
