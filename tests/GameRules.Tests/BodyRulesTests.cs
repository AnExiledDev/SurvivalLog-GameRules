using System.Linq;
using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class XpScaleTests
{
    [Fact]
    public void At_the_games_rate_every_gain_is_untouched()
    {
        var xp = new XpScale();

        Assert.Equal(7, xp.Base(skill: 1, exp: 7));
        Assert.Equal(7, xp.Discovery(7));
    }

    [Fact]
    public void A_whole_multiplier_multiplies_each_gain()
    {
        var xp = new XpScale();

        xp.Use(3f);

        Assert.Equal(15, xp.Base(1, 5));
    }

    [Fact]
    public void Small_gains_at_a_fractional_rate_add_up_to_the_right_total()
    {
        var xp = new XpScale();

        xp.Use(1.5f);

        var gains = Enumerable.Range(0, 10).Select(_ => xp.Base(1, 1)).ToList();

        Assert.Equal(new[] { 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 }, gains);
    }

    [Fact]
    public void A_low_rate_still_pays_out_once_the_parts_reach_a_whole_point()
    {
        var xp = new XpScale();

        xp.Use(0.1f);

        var total = Enumerable.Range(0, 30).Sum(_ => xp.Base(1, 1));

        Assert.Equal(3, total);
    }

    [Fact]
    public void Each_skill_keeps_its_own_left_over_part()
    {
        var xp = new XpScale();

        xp.Use(0.5f);
        xp.Base(skill: 1, exp: 1);

        Assert.Equal(0, xp.Base(skill: 2, exp: 1));
        Assert.Equal(1, xp.Base(skill: 1, exp: 1));
    }

    [Fact]
    public void A_new_rate_drops_the_old_rates_left_over_part()
    {
        var xp = new XpScale();

        xp.Use(0.5f);
        xp.Base(1, 1);
        xp.Use(0.6f);

        Assert.Equal(0, xp.Base(1, 1));
    }

    [Fact]
    public void Zero_gives_no_xp_at_all()
    {
        var xp = new XpScale();

        xp.Use(0f);

        Assert.Equal(0, xp.Base(1, 50));
        Assert.Equal(0, xp.Discovery(50));
    }

    [Fact]
    public void A_discovery_is_rounded_but_never_to_nothing()
    {
        var xp = new XpScale();

        xp.Use(0.1f);

        Assert.Equal(1, xp.Discovery(3));
        Assert.Equal(2, xp.Discovery(15));
    }

    [Fact]
    public void Back_at_the_games_rate_nothing_left_over_is_paid()
    {
        var xp = new XpScale();

        xp.Use(0.5f);
        xp.Base(1, 1);
        xp.Use(1f);

        Assert.Equal(1, xp.Base(1, 1));
    }

    [Fact]
    public void Xp_carried_over_from_an_earlier_run_is_not_scaled_again()
    {
        Assert.False(XpScale.Scales(XpScale.InheritSource));
        Assert.True(XpScale.Scales(0));
    }
}

public sealed class StaminaChangeTests
{
    private const float Cost = 0.5f;
    private const float Strength = 2f;

    [Theory]
    [InlineData(StaminaChange.ArgsAction, StaminaChange.SourceAction)]
    [InlineData(StaminaChange.ArgsNone, StaminaChange.SourceFurniture)]
    [InlineData(StaminaChange.ArgsNone, StaminaChange.SourceNone)]
    public void A_cost_from_an_action_is_scaled(int args, int source)
    {
        Assert.Equal(Cost, StaminaChange.Factor(-10f, args, source, 0, Cost, Strength));
    }

    [Fact]
    public void Stamina_lost_over_time_is_not_an_action_cost()
    {
        const int natural = 3;

        Assert.Equal(1f, StaminaChange.Factor(-1f, natural, StaminaChange.SourceNone, 0, Cost, Strength));
    }

    [Fact]
    public void Stamina_a_buff_takes_is_not_an_action_cost()
    {
        Assert.Equal(1f, StaminaChange.Factor(-5f, StaminaChange.ArgsNone, StaminaChange.SourceBuff, 24030, Cost, Strength));
    }

    [Fact]
    public void A_stimulant_drink_gives_its_scaled_stamina()
    {
        const int sportsDrink = 2131;

        Assert.Equal(Strength, StaminaChange.Factor(40f, StaminaChange.ArgsNone, StaminaChange.SourceItem, sportsDrink, Cost, Strength));
    }

    [Fact]
    public void A_stimulant_buff_ticking_gives_its_scaled_stamina()
    {
        const int stimulantsBuff = 24040;

        Assert.Equal(Strength, StaminaChange.Factor(5f, StaminaChange.ArgsNone, StaminaChange.SourceBuff, stimulantsBuff, Cost, Strength));
    }

    [Fact]
    public void Stamina_from_food_or_sleep_is_left_alone()
    {
        const int cannedFood = 2001;

        Assert.Equal(1f, StaminaChange.Factor(10f, StaminaChange.ArgsNone, StaminaChange.SourceItem, cannedFood, Cost, Strength));
        Assert.Equal(1f, StaminaChange.Factor(5f, StaminaChange.ArgsAction, StaminaChange.SourceAction, 5, Cost, Strength));
    }

    [Fact]
    public void A_stimulant_id_only_counts_from_its_own_source()
    {
        const int sportsDrink = 2131;

        Assert.Equal(1f, StaminaChange.Factor(5f, StaminaChange.ArgsNone, StaminaChange.SourceBuff, sportsDrink, Cost, Strength));
    }

    [Fact]
    public void A_cost_is_never_treated_as_a_stimulant_gain()
    {
        const int adrenaline = 2403;

        Assert.Equal(1f, StaminaChange.Factor(-5f, StaminaChange.ArgsNone, StaminaChange.SourceItem, adrenaline, Cost, Strength));
    }
}

public sealed class CapKeeperTests
{
    [Fact]
    public void The_first_check_adds_the_whole_extra()
    {
        var keeper = new CapKeeper();

        Assert.Equal(50, keeper.Next(character: 1, wanted: 50));
    }

    [Fact]
    public void Later_checks_add_nothing_more()
    {
        var keeper = new CapKeeper();

        keeper.Next(1, 50);

        Assert.Equal(0, keeper.Next(1, 50));
    }

    [Fact]
    public void A_changed_setting_adds_or_takes_only_the_difference()
    {
        var keeper = new CapKeeper();

        keeper.Next(1, 50);

        Assert.Equal(30, keeper.Next(1, 80));
        Assert.Equal(-100, keeper.Next(1, -20));
    }

    [Fact]
    public void Back_at_zero_everything_the_mod_added_is_taken_back()
    {
        var keeper = new CapKeeper();

        keeper.Next(1, 50);
        keeper.Next(1, 80);

        Assert.Equal(-80, keeper.Next(1, 0));
        Assert.Equal(0, keeper.Added);
    }

    [Fact]
    public void A_different_character_gets_the_whole_extra()
    {
        var keeper = new CapKeeper();

        keeper.Next(1, 50);

        Assert.Equal(50, keeper.Next(2, 50));
    }

    [Fact]
    public void A_cleared_character_gets_the_whole_extra_again()
    {
        var keeper = new CapKeeper();

        keeper.Next(1, 50);
        keeper.Forget(1);

        Assert.Equal(50, keeper.Next(1, 50));
    }

    [Fact]
    public void Clearing_some_other_character_changes_nothing()
    {
        var keeper = new CapKeeper();

        keeper.Next(1, 50);
        keeper.Forget(2);

        Assert.Equal(0, keeper.Next(1, 50));
    }
}

public sealed class BodySettingsTests
{
    [Fact]
    public void The_games_rules_set_no_daily_limit()
    {
        Assert.Empty(BodySettings.Game.DailyLimitChanges());
    }

    [Fact]
    public void A_daily_limit_goes_on_every_stimulant()
    {
        var changes = (BodySettings.Game with { StimulantDailyLimit = 2 }).DailyLimitChanges();

        Assert.Equal(Stimulants.Items.OrderBy(id => id), changes.Keys.OrderBy(id => id));
        Assert.All(changes.Values, change => Assert.Equal(2f, change.Apply(0f)));
    }

    [Fact]
    public void Each_limit_has_its_own_key()
    {
        var changes = (BodySettings.Game with { StimulantDailyLimit = 1 }).DailyLimitChanges();

        Assert.Equal(changes.Count, changes.Values.Select(change => change.Key).Distinct().Count());
        Assert.Equal("DailyMax_2131", changes[2131].Key);
    }

    [Fact]
    public void Turning_the_limit_off_puts_the_games_no_limit_back()
    {
        var keeper = new TableKeeper();
        var key = BodySettings.DailyLimitKey(2131);
        var limited = (BodySettings.Game with { StimulantDailyLimit = 2 }).DailyLimitChanges();

        keeper.Target(key, entryId: 1, entryValue: 0f, limited[2131]);

        BodySettings.Game.DailyLimitChanges().TryGetValue(2131, out var off);

        Assert.Equal(0f, keeper.Target(key, 1, 2f, off));
    }
}
