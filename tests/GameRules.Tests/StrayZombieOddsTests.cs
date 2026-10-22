using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class StrayZombieOddsTests
{
    private const string GameCurve = "15:0;9999:0.3";

    [Fact]
    public void The_games_chance_leaves_the_curve_as_the_game_wrote_it()
    {
        Assert.Equal(GameCurve, StrayZombieOdds.LeaderCurve(GameCurve, 1f));
    }

    [Fact]
    public void A_multiplier_scales_every_chance_and_keeps_the_days()
    {
        Assert.Equal("15:0;9999:0.6", StrayZombieOdds.LeaderCurve(GameCurve, 2f));
        Assert.Equal("15:0;9999:0.15", StrayZombieOdds.LeaderCurve(GameCurve, 0.5f));
    }

    [Fact]
    public void A_chance_never_goes_past_certain()
    {
        Assert.Equal("15:0;9999:1", StrayZombieOdds.LeaderCurve(GameCurve, 4f));
    }

    [Fact]
    public void Zero_means_no_visit_is_ever_led()
    {
        Assert.Equal("15:0;9999:0", StrayZombieOdds.LeaderCurve(GameCurve, 0f));
    }

    [Theory]
    [InlineData("")]
    [InlineData("15:0;9999")]
    [InlineData("15:0;9999:high")]
    [InlineData("15:0:1")]
    public void A_curve_that_does_not_read_keeps_the_games_rule(string curve)
    {
        Assert.Equal(curve, StrayZombieOdds.LeaderCurve(curve, 2f));
    }

    [Fact]
    public void Whole_loot_multipliers_always_roll_that_many_times()
    {
        Assert.Equal(1, StrayZombieOdds.LootRolls(1f, 0.99));
        Assert.Equal(2, StrayZombieOdds.LootRolls(2f, 0.0));
        Assert.Equal(2, StrayZombieOdds.LootRolls(2f, 0.99));
    }

    [Fact]
    public void A_fraction_adds_one_more_roll_that_share_of_the_time()
    {
        Assert.Equal(2, StrayZombieOdds.LootRolls(1.5f, 0.4));
        Assert.Equal(1, StrayZombieOdds.LootRolls(1.5f, 0.6));
        Assert.Equal(1, StrayZombieOdds.LootRolls(0.5f, 0.4));
        Assert.Equal(0, StrayZombieOdds.LootRolls(0.5f, 0.6));
    }

    [Fact]
    public void No_loot_never_rolls()
    {
        Assert.Equal(0, StrayZombieOdds.LootRolls(0f, 0.0));
    }
}
