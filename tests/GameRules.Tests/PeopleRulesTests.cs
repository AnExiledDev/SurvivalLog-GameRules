using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class DeathGuardTests
{
    [Fact]
    public void Under_the_games_rules_she_can_die_and_the_hour_stops()
    {
        var guard = new DeathGuard();

        guard.CheckStarts();

        Assert.True(guard.AllowsDeath());
        Assert.True(guard.CheckResult(true));
    }

    [Fact]
    public void Never_die_stops_the_death_and_lets_the_hour_go_on()
    {
        var guard = new DeathGuard();

        guard.Use(neverDie: true);
        guard.CheckStarts();

        Assert.False(guard.AllowsDeath());
        Assert.False(guard.CheckResult(true));
    }

    [Fact]
    public void A_stopped_death_does_not_hide_a_later_hours_result()
    {
        var guard = new DeathGuard();

        guard.Use(neverDie: true);
        guard.CheckStarts();
        guard.AllowsDeath();
        guard.Use(neverDie: false);
        guard.CheckStarts();

        Assert.True(guard.CheckResult(true));
    }
}

public sealed class MishapOddsTests
{
    [Fact]
    public void The_setting_multiplies_the_days_multiplier()
    {
        Assert.Equal(3f, MishapOdds.Multiplier(1.5f, 2f));
    }

    [Fact]
    public void A_day_with_no_multiplier_counts_as_1_like_the_game()
    {
        Assert.Equal(0.5f, MishapOdds.Multiplier(0f, 0.5f));
    }

    [Fact]
    public void Only_a_setting_of_0_skips_the_roll()
    {
        Assert.True(MishapOdds.SkipsRoll(0f));
        Assert.False(MishapOdds.SkipsRoll(0.1f));
    }
}

public sealed class IntEntryTests
{
    [Fact]
    public void Params2_that_mirrors_params1_is_kept_in_step()
    {
        Assert.True(IntEntry.KeepsParams2InStep(10, 10f));
    }

    [Fact]
    public void Params2_with_its_own_meaning_is_left_alone()
    {
        Assert.False(IntEntry.KeepsParams2InStep(3600, 0f));
    }
}
