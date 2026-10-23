using System.Linq;
using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class PeopleSettingsTests
{
    [Fact]
    public void The_games_rules_change_no_table_value()
    {
        Assert.Empty(PeopleSettings.Game.TableChanges());
    }

    [Fact]
    public void A_later_rescue_deadline_moves_both_the_neighbour_and_the_companion()
    {
        var changes = (PeopleSettings.Game with { RescueDeadline = 40 }).TableChanges().ToList();

        Assert.Equal(40f, Value(changes, "Neighbor_DeathDayBase", 21f));
        Assert.Equal(40f, Value(changes, "Companion_DeathDayBase", 21f));
    }

    [Fact]
    public void No_trust_loss_zeroes_hunger_and_letter_losses()
    {
        var changes = (PeopleSettings.Game with { TrustLoss = 0f }).TableChanges().ToList();

        Assert.Equal(0f, Value(changes, "Trust_HungerLoss", 3f));
        Assert.Equal(0f, Value(changes, "Trust_IgnoreLoss", 2f));
    }

    [Fact]
    public void A_mishap_turned_off_gets_no_weight_and_the_others_keep_theirs()
    {
        var changes = (PeopleSettings.Game with { MishapsOff = new[] { Mishap.Fire } }).TableChanges().ToList();

        Assert.Equal(0f, Value(changes, "Stranger_MishapW_Fire", 8f));
        Assert.DoesNotContain(changes, change => change.Key == "Stranger_MishapW_Rat");
    }

    [Fact]
    public void More_frequent_thoughts_shorten_the_wait_between_them()
    {
        var changes = (PeopleSettings.Game with { ThoughtFrequency = 2f }).TableChanges().ToList();

        Assert.Equal(1800f, Value(changes, "ThinkCD", 3600f));
        Assert.Equal(1800f, Value(changes, "ThinkIdleTime", 3600f));
        Assert.Equal(1800f, Value(changes, "GuideRandomTalk", 3600f));
    }

    [Fact]
    public void Morale_gains_scale_music_and_perfect_harvest_morale()
    {
        var changes = (PeopleSettings.Game with { MoraleGains = 2f }).TableChanges().ToList();

        Assert.Equal(20f, Value(changes, "Music_TrackFinish_Morale", 10f));
        Assert.Equal(4f, Value(changes, "Plant_Harvest_Perfect_Morale", 2f));
    }

    [Fact]
    public void Full_crafting_xp_gives_full_experience_one_to_four_levels_below()
    {
        var changes = (PeopleSettings.Game with { FullCraftingXp = true }).TableChanges().ToList();

        Assert.Equal(1f, Value(changes, "crafting_exp_reduction_params_01", 0.8f));
        Assert.Equal(1f, Value(changes, "crafting_exp_reduction_params_04", 0.2f));
    }

    [Fact]
    public void Commission_aid_reach_and_vase_settings_scale_their_own_values()
    {
        var settings = PeopleSettings.Game with { CommissionChance = 0.5f, AidPlatformChance = 1.4f, PickupReach = 2f, VaseMoraleCap = 3f };
        var changes = settings.TableChanges().ToList();

        Assert.Equal(0.35f, Value(changes, "Commission_DailyChance", 0.7f), 4);
        Assert.Equal(0.98f, Value(changes, "AidPlatform_DailyChance", 0.7f), 4);
        Assert.Equal(4f, Value(changes, "PickupRange", 2f));
        Assert.Equal(3f, Value(changes, "Vase_MoraleCap", 1f));
    }

    [Fact]
    public void A_slower_decline_stretches_the_hours_between_condition_drops_for_both()
    {
        var changes = (PeopleSettings.Game with { DeclineTime = 2f }).TableChanges().ToList();

        Assert.Equal(96f, Value(changes, "Neighbor_TierDropHours", 48f));
        Assert.Equal(96f, Value(changes, "Companion_TierDropHours", 48f));
    }

    [Fact]
    public void A_faster_basket_shortens_every_return_delay_including_weak_and_dying()
    {
        var changes = (PeopleSettings.Game with { BasketReturnTime = 0.5f }).TableChanges().ToList();

        Assert.Equal(8f, Value(changes, "Neighbor_ReturnDelayHourMin", 16f));
        Assert.Equal(9f, Value(changes, "Neighbor_ReturnDelayHourMax", 18f));
        Assert.Equal(4f, Value(changes, "Neighbor_WeakReturnDelayHour", 8f));
        Assert.Equal(1f, Value(changes, "Neighbor_DyingReturnDelayHour", 2f));
        Assert.Equal(4f, Value(changes, "Companion_WeakReturnDelayHour", 8f));
        Assert.Equal(1f, Value(changes, "Companion_DyingReturnDelayHour", 2f));
    }

    [Fact]
    public void No_crises_zeroes_the_crisis_chance()
    {
        var changes = (PeopleSettings.Game with { CrisisChance = 0f }).TableChanges().ToList();

        Assert.Equal(0f, Value(changes, "Neighbor_CrisisProb", 0.25f));
    }

    [Fact]
    public void More_return_gifts_divide_the_sent_value_by_less()
    {
        var changes = (PeopleSettings.Game with { ReturnGifts = 2f }).TableChanges().ToList();

        Assert.Equal(5f, Value(changes, "Neighbor_GiftValuePerReturn", 10f));
    }

    [Fact]
    public void Dislike_penalty_and_struggle_count_are_set_for_both()
    {
        var changes = (PeopleSettings.Game with { DislikePenalty = 0, StruggleCount = 5 }).TableChanges().ToList();

        Assert.Equal(0f, Value(changes, "Neighbor_DislikeAffinityPenalty", 1f));
        Assert.Equal(0f, Value(changes, "Companion_DislikeAffinityPenalty", 1f));
        Assert.Equal(5f, Value(changes, "Neighbor_StruggleMaxCount", 3f));
        Assert.Equal(5f, Value(changes, "Companion_StruggleMaxCount", 3f));
    }

    private static float Value(System.Collections.Generic.List<TableChange> changes, string key, float gameValue) =>
        changes.Single(change => change.Key == key).Apply(gameValue);
}
