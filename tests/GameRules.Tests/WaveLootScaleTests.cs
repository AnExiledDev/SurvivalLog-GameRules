using System.Collections.Generic;
using System.Linq;
using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class WaveLootScaleTests
{
    [Fact]
    public void The_games_loot_changes_no_table_value()
    {
        Assert.Empty(WaveLootScale.Changes(1f));
    }

    [Fact]
    public void Double_loot_halves_the_kills_per_step_and_doubles_the_limits()
    {
        var changes = WaveLootScale.Changes(2f).ToList();

        Assert.Equal(10f, Value(changes, "TowerWaveLootCatPerKill", 20f));
        Assert.Equal(30f, Value(changes, "TowerWaveLootQtyPerKill", 60f));
        Assert.Equal(16f, Value(changes, "TowerWaveLootCatMax", 8f));
        Assert.Equal(4f, Value(changes, "TowerWaveLootQtyMax", 2f));
    }

    [Fact]
    public void Half_loot_needs_twice_the_kills_per_step_and_halves_the_limits()
    {
        var changes = WaveLootScale.Changes(0.5f).ToList();

        Assert.Equal(40f, Value(changes, "TowerWaveLootCatPerKill", 20f));
        Assert.Equal(120f, Value(changes, "TowerWaveLootQtyPerKill", 60f));
        Assert.Equal(4f, Value(changes, "TowerWaveLootCatMax", 8f));
        Assert.Equal(1f, Value(changes, "TowerWaveLootQtyMax", 2f));
    }

    [Fact]
    public void A_scaled_value_never_drops_to_zero_where_the_game_would_fall_back_to_its_own()
    {
        Assert.Equal(1f, new WholeScaleBy("TowerWaveLootQtyMax", 0.1f).Apply(2f));
    }

    [Fact]
    public void A_half_rounds_up_rather_than_to_even()
    {
        Assert.Equal(3f, new WholeScaleBy("TowerWaveLootCatMax", 0.5f).Apply(5f));
    }

    private static float Value(List<TableChange> changes, string key, float gameValue) =>
        changes.Single(change => change.Key == key).Apply(gameValue);
}
