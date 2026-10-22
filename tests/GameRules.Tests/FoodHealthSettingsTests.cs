using System.Collections.Generic;
using System.Linq;
using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class FoodHealthSettingsTests
{
    [Fact]
    public void The_games_rules_change_no_table_value_and_no_sickness_chance()
    {
        Assert.Empty(FoodHealthSettings.Game.TableChanges());
        Assert.Empty(FoodHealthSettings.Game.EffectChanges());
    }

    [Fact]
    public void A_raw_food_chance_scaled_past_certainty_stays_at_certainty()
    {
        var changes = (FoodHealthSettings.Game with { RawFoodSickChance = 5f }).TableChanges().ToList();

        Assert.Equal(0.75f, Value(changes, "RawFoodDebuffRate", 0.15f), 4);
        Assert.Equal(1f, Value(changes, "RawFoodDebuffRate", 0.3f));
    }

    [Fact]
    public void No_raw_food_sickness_makes_the_chance_zero()
    {
        var changes = (FoodHealthSettings.Game with { RawFoodSickChance = 0f }).TableChanges().ToList();

        Assert.Equal(0f, Value(changes, "RawFoodDebuffRate", 0.15f));
    }

    [Fact]
    public void A_shorter_food_fatigue_shortens_its_start_its_growth_its_cap_and_its_recovery()
    {
        var changes = (FoodHealthSettings.Game with { FoodFatigueLength = 0.5f }).TableChanges().ToList();

        Assert.Equal(43200f, Value(changes, "TiredOfFoodTime", 86400f));
        Assert.Equal(21600f, Value(changes, "TiredOfFoodTimeAdd", 43200f));
        Assert.Equal(86400f, Value(changes, "TiredOfFoodTimeMax", 172800f));
        Assert.Equal(10800f, Value(changes, "TiredOfFoodTimeReduce", 21600f));
    }

    [Fact]
    public void Double_the_fatigue_penalty_takes_away_everything_a_tired_food_gives()
    {
        var changes = (FoodHealthSettings.Game with { FoodFatiguePenalty = 2f }).TableChanges().ToList();

        Assert.Equal(-1f, Value(changes, "TiredOfFood2", -0.5f));
    }

    [Fact]
    public void A_mold_count_left_at_the_games_value_is_not_written()
    {
        var settings = FoodHealthSettings.Game with { MoldSprayRadius = 3 };
        var changes = settings.TableChanges().ToList();

        Assert.Equal(3f, Value(changes, "FoodPollution_SprayRadius", 1f));
        Assert.DoesNotContain(changes, change => change.Key == "FoodPollution_SprayItemCost");
        Assert.DoesNotContain(changes, change => change.Key == "FoodPollution_SpreadItemsMax");
        Assert.DoesNotContain(changes, change => change.Key == "FoodPollution_SeedCountDay1");
    }

    [Fact]
    public void Every_mold_count_moved_off_the_games_value_is_written()
    {
        var settings = FoodHealthSettings.Game with { MoldSprayCost = 2, MoldSpreadItems = 1, MoldSeedsDay1 = 6 };
        var changes = settings.TableChanges().ToList();

        Assert.Equal(2f, Value(changes, "FoodPollution_SprayItemCost", 1f));
        Assert.Equal(1f, Value(changes, "FoodPollution_SpreadItemsMax", 2f));
        Assert.Equal(6f, Value(changes, "FoodPollution_SeedCountDay1", 3f));
    }

    [Fact]
    public void A_longer_spread_interval_lengthens_both_the_wait_and_its_floor()
    {
        var changes = (FoodHealthSettings.Game with { MoldSpreadInterval = 2f }).TableChanges().ToList();

        Assert.Equal(16f, Value(changes, "FoodPollution_SpreadIntervalHours", 8f));
        Assert.Equal(6f, Value(changes, "FoodPollution_SpreadIntervalMin", 3f));
    }

    [Fact]
    public void Slower_moldy_spoiling_scales_the_moldy_decay_rate()
    {
        var changes = (FoodHealthSettings.Game with { MoldyFoodSpoilSpeed = 0.2f }).TableChanges().ToList();

        Assert.Equal(1f, Value(changes, "FoodPollution_ItemDecayScale", 5f), 4);
    }

    [Fact]
    public void Rat_visit_chance_scales_the_base_the_food_step_and_the_cap_and_never_passes_certainty()
    {
        var changes = (FoodHealthSettings.Game with { RatVisitChance = 3f }).TableChanges().ToList();

        Assert.Equal(0.15f, Value(changes, "RatVisit_BaseChance", 0.05f), 4);
        Assert.Equal(0.15f, Value(changes, "RatVisit_StepChance", 0.05f), 4);
        Assert.Equal(0.9f, Value(changes, "RatVisit_MaxChance", 0.3f), 4);
        Assert.Equal(1f, Value(changes, "RatVisit_MaxChance", 0.5f));
    }

    [Fact]
    public void No_rat_visits_make_every_rat_chance_zero()
    {
        var changes = (FoodHealthSettings.Game with { RatVisitChance = 0f }).TableChanges().ToList();

        Assert.Equal(0f, Value(changes, "RatVisit_BaseChance", 0.05f));
        Assert.Equal(0f, Value(changes, "RatVisit_StepChance", 0.05f));
        Assert.Equal(0f, Value(changes, "RatVisit_MaxChance", 0.3f));
    }

    [Fact]
    public void A_longer_rat_escape_time_gives_more_time_to_catch_it()
    {
        var changes = (FoodHealthSettings.Game with { RatEscapeTime = 2f }).TableChanges().ToList();

        Assert.Equal(8f, Value(changes, "RatVisit_EscapeSeconds", 4f));
    }

    [Fact]
    public void Each_cold_sickness_chance_changes_only_its_own_effect_and_stays_within_certainty()
    {
        var changes = (FoodHealthSettings.Game with { WindChillChance = 2f }).EffectChanges();

        Assert.False(changes.ContainsKey(FoodHealthSettings.FeverEffect));
        Assert.Equal(1f, changes[FoodHealthSettings.WindChillEffect].Apply(0.6f));
    }

    [Fact]
    public void No_fever_makes_its_chance_zero_and_leaves_wind_chill_alone()
    {
        var changes = (FoodHealthSettings.Game with { FeverChance = 0f }).EffectChanges();

        Assert.Equal(0f, changes[FoodHealthSettings.FeverEffect].Apply(0.5f));
        Assert.False(changes.ContainsKey(FoodHealthSettings.WindChillEffect));
    }

    [Fact]
    public void The_switch_settings_change_no_table_value()
    {
        var settings = FoodHealthSettings.Game with { NoFoodFatigue = true, NoPlague = true, NoPlagueWorsening = true, NoBlackoutPanic = true };

        Assert.Empty(settings.TableChanges());
        Assert.Empty(settings.EffectChanges());
    }

    private static float Value(List<TableChange> changes, string key, float gameValue) =>
        changes.Single(change => change.Key == key).Apply(gameValue);
}
