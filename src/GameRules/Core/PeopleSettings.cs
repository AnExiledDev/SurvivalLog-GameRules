using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>The mishaps a supported stranger can suffer, each with its own weight in the game's table.</summary>
public enum Mishap
{
    Rat,
    Fire,
    Theft,
    Facility,
    Pest,
    Injury,
}

/// <summary>
/// The settings for the neighbour and companion, supported strangers, commissions, thoughts, morale, crafting and
/// reach. Multipliers of 1 and switches that are off are the game's own rules and change nothing.
/// </summary>
public sealed record PeopleSettings(
    bool NeverDie,
    int RescueDeadline,
    float TrustLoss,
    float MishapChance,
    IReadOnlyCollection<Mishap> MishapsOff,
    float CommissionChance,
    float AidPlatformChance,
    float ThoughtFrequency,
    float MoraleGains,
    float VaseMoraleCap,
    bool FullCraftingXp,
    float PickupReach,
    float DeclineTime,
    float BasketReturnTime,
    float CrisisChance,
    int DislikePenalty,
    int StruggleCount,
    float ReturnGifts)
{
    /// <summary>The day after the disaster the neighbour and companion die on if their rescue never started.</summary>
    public const int GameRescueDeadline = 21;

    /// <summary>Affinity lost when she is sent food she dislikes.</summary>
    public const int GameDislikePenalty = 1;

    /// <summary>How many times she can pull through near death before the death day takes her.</summary>
    public const int GameStruggleCount = 3;

    public static PeopleSettings Game { get; } = new(
        NeverDie: false,
        RescueDeadline: GameRescueDeadline,
        TrustLoss: 1f,
        MishapChance: 1f,
        MishapsOff: new Mishap[0],
        CommissionChance: 1f,
        AidPlatformChance: 1f,
        ThoughtFrequency: 1f,
        MoraleGains: 1f,
        VaseMoraleCap: 1f,
        FullCraftingXp: false,
        PickupReach: 1f,
        DeclineTime: 1f,
        BasketReturnTime: 1f,
        CrisisChance: 1f,
        DislikePenalty: GameDislikePenalty,
        StruggleCount: GameStruggleCount,
        ReturnGifts: 1f);

    /// <summary>
    /// The GlobalSetting values these settings change. Thoughts come more often by waiting less between them, and more
    /// return gifts come by dividing the sent value by less.
    /// </summary>
    public IEnumerable<TableChange> TableChanges()
    {
        if (RescueDeadline != GameRescueDeadline)
        {
            yield return new SetTo("Neighbor_DeathDayBase", RescueDeadline);
            yield return new SetTo("Companion_DeathDayBase", RescueDeadline);
        }

        foreach (var change in Scale(TrustLoss, "Trust_HungerLoss", "Trust_IgnoreLoss", "Trust_DailyLossCap")) yield return change;

        foreach (var mishap in MishapsOff) yield return new SetTo("Stranger_MishapW_" + mishap, 0f);

        foreach (var change in Scale(CommissionChance, "Commission_DailyChance")) yield return change;
        foreach (var change in Scale(AidPlatformChance, "AidPlatform_DailyChance")) yield return change;
        foreach (var change in Scale(1f / ThoughtFrequency, "ThinkCD", "ThinkIdleTime", "GuideRandomTalk")) yield return change;
        foreach (var change in Scale(MoraleGains, "Music_TrackFinish_Morale", "Plant_Harvest_Perfect_Morale")) yield return change;
        foreach (var change in Scale(VaseMoraleCap, "Vase_MoraleCap")) yield return change;
        foreach (var change in Scale(PickupReach, "PickupRange")) yield return change;
        foreach (var change in NeighborChanges()) yield return change;

        if (!FullCraftingXp) yield break;

        foreach (var level in new[] { "01", "02", "03", "04" }) yield return new SetTo("crafting_exp_reduction_params_" + level, 1f);
    }

    private IEnumerable<TableChange> NeighborChanges()
    {
        foreach (var change in Scale(DeclineTime, "Neighbor_TierDropHours", "Companion_TierDropHours")) yield return change;

        var delays = new[]
        {
            "Neighbor_ReturnDelayHourMin", "Neighbor_ReturnDelayHourMax", "Neighbor_WeakReturnDelayHour",
            "Neighbor_DyingReturnDelayHour", "Companion_WeakReturnDelayHour", "Companion_DyingReturnDelayHour",
        };

        foreach (var change in Scale(BasketReturnTime, delays)) yield return change;
        foreach (var change in Scale(CrisisChance, "Neighbor_CrisisProb")) yield return change;
        foreach (var change in Scale(1f / ReturnGifts, "Neighbor_GiftValuePerReturn")) yield return change;

        if (DislikePenalty != GameDislikePenalty)
        {
            yield return new SetTo("Neighbor_DislikeAffinityPenalty", DislikePenalty);
            yield return new SetTo("Companion_DislikeAffinityPenalty", DislikePenalty);
        }

        if (StruggleCount == GameStruggleCount) yield break;

        yield return new SetTo("Neighbor_StruggleMaxCount", StruggleCount);
        yield return new SetTo("Companion_StruggleMaxCount", StruggleCount);
    }

    private static IEnumerable<TableChange> Scale(float factor, params string[] keys)
    {
        if (factor == 1f) yield break;

        foreach (var key in keys) yield return new ScaleBy(key, factor);
    }
}
