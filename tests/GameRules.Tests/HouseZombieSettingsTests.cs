using System.Collections.Generic;
using System.Linq;
using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class HouseZombieSettingsTests
{
    [Fact]
    public void The_games_rules_change_no_table_value_and_need_no_hook()
    {
        var game = HouseZombieSettings.Game;

        Assert.Empty(game.TableChanges());
        Assert.False(game.Events.BlocksAny);
        Assert.False(game.RerollsStrayLoot);
    }

    [Fact]
    public void No_stray_zombies_turns_them_off()
    {
        var changes = (HouseZombieSettings.Game with { NoStrayZombies = true }).TableChanges().ToList();

        Assert.Equal(0f, Value(changes, "StrayZombieEnabled", 1f));
    }

    [Fact]
    public void Stray_zombie_days_hours_and_counts_replace_the_games_values()
    {
        var settings = HouseZombieSettings.Game with
        {
            StrayZombieStartDay = 20,
            StrayZombieCooldownHours = 0,
            StrayZombieMaxPerDay = 6,
            StrayZombieMaxAlive = 10,
        };
        var changes = settings.TableChanges().ToList();

        Assert.Equal(20f, Value(changes, "StrayZombieMinDay", 7f));
        Assert.Equal(0f, Value(changes, "StrayZombieCooldownHours", 4f));
        Assert.Equal(6f, Value(changes, "StrayZombieMaxPerDay", 3f));
        Assert.Equal(10f, Value(changes, "StrayZombieMaxAlive", 3f));
    }

    [Fact]
    public void A_stray_zombie_value_left_at_the_games_own_writes_nothing()
    {
        var changes = (HouseZombieSettings.Game with { StrayZombieMaxAlive = 5 }).TableChanges().ToList();

        Assert.DoesNotContain(changes, change => change.Key == "StrayZombieMinDay");
        Assert.DoesNotContain(changes, change => change.Key == "StrayZombieMaxPerDay");
    }

    [Fact]
    public void No_stray_zombie_loot_empties_the_loot_group_without_a_hook()
    {
        var settings = HouseZombieSettings.Game with { StrayZombieLoot = 0f };

        Assert.Equal(0f, Value(settings.TableChanges().ToList(), "StrayZombieLootGroupId", 8057f));
        Assert.False(settings.RerollsStrayLoot);
    }

    [Fact]
    public void More_stray_zombie_loot_rolls_again_and_keeps_the_games_loot_group()
    {
        var settings = HouseZombieSettings.Game with { StrayZombieLoot = 2f };

        Assert.True(settings.RerollsStrayLoot);
        Assert.DoesNotContain(settings.TableChanges(), change => change.Key == "StrayZombieLootGroupId");
    }

    [Fact]
    public void No_adaptive_waves_turns_the_adaptive_difficulty_off()
    {
        var changes = (HouseZombieSettings.Game with { NoAdaptiveWaves = true }).TableChanges().ToList();

        Assert.Equal(0f, Value(changes, "SiegeElastic_Enable", 1f));
    }

    [Fact]
    public void More_wave_loot_reaches_the_wave_loot_table_values()
    {
        var changes = (HouseZombieSettings.Game with { WaveLoot = 2f }).TableChanges().ToList();

        Assert.Equal(10f, Value(changes, "TowerWaveLootCatPerKill", 20f));
        Assert.Equal(4f, Value(changes, "TowerWaveLootQtyMax", 2f));
    }

    [Fact]
    public void The_storm_setting_reaches_the_event_block()
    {
        var settings = HouseZombieSettings.Game with { NoStormLeaks = true };

        Assert.Equal(new HouseEventBlock(NoStormLeaks: true), settings.Events);
    }

    private static float Value(List<TableChange> changes, string key, float gameValue) =>
        changes.Single(change => change.Key == key).Apply(gameValue);
}
