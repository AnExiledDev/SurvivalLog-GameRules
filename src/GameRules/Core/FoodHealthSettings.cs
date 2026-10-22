using System;
using System.Collections.Generic;
using System.Globalization;

namespace GameRules.Core;

/// <summary>
/// The settings for raw food, food fatigue, mold, rats, sickness and blackout panic. Multipliers of 1, switches that
/// are off and the game's own counts are the game's rules and change nothing.
/// </summary>
public sealed record FoodHealthSettings(
    float RawFoodSickChance,
    bool NoFoodFatigue,
    float FoodFatigueLength,
    float FoodFatiguePenalty,
    int MoldSprayRadius,
    int MoldSprayCost,
    float MoldyFoodSpoilSpeed,
    float MoldSpreadInterval,
    int MoldSpreadItems,
    int MoldSeedsDay1,
    float RatVisitChance,
    float RatEscapeTime,
    bool NoPlague,
    bool NoPlagueWorsening,
    float WindChillChance,
    float FeverChance,
    bool NoBlackoutPanic)
{
    public const int GameSprayRadius = 1;
    public const int GameSprayCost = 1;
    public const int GameSpreadItems = 2;
    public const int GameSeedsDay1 = 3;

    /// <summary>The Config_Effect row whose FloatValue0 is the chance of catching Wind Chill in a cold wave without heat.</summary>
    public const int WindChillEffect = 200010011;

    /// <summary>The Config_Effect row whose FloatValue0 is the chance of catching a High Temperature in a cold wave without heat.</summary>
    public const int FeverEffect = 200010012;

    public static IReadOnlyList<int> SicknessEffects { get; } = new[] { WindChillEffect, FeverEffect };

    public static FoodHealthSettings Game { get; } = new(
        RawFoodSickChance: 1f,
        NoFoodFatigue: false,
        FoodFatigueLength: 1f,
        FoodFatiguePenalty: 1f,
        MoldSprayRadius: GameSprayRadius,
        MoldSprayCost: GameSprayCost,
        MoldyFoodSpoilSpeed: 1f,
        MoldSpreadInterval: 1f,
        MoldSpreadItems: GameSpreadItems,
        MoldSeedsDay1: GameSeedsDay1,
        RatVisitChance: 1f,
        RatEscapeTime: 1f,
        NoPlague: false,
        NoPlagueWorsening: false,
        WindChillChance: 1f,
        FeverChance: 1f,
        NoBlackoutPanic: false);

    public PlagueBlock PlagueBlock => new(NoPlague, NoPlagueWorsening);

    /// <summary>The GlobalSetting values these settings change. Chances stay between 0 and 1 however far they are scaled.</summary>
    public IEnumerable<TableChange> TableChanges()
    {
        foreach (var change in ScaleChance(RawFoodSickChance, "RawFoodDebuffRate")) yield return change;
        foreach (var change in Scale(FoodFatigueLength, "TiredOfFoodTime", "TiredOfFoodTimeAdd", "TiredOfFoodTimeMax", "TiredOfFoodTimeReduce")) yield return change;
        foreach (var change in Scale(FoodFatiguePenalty, "TiredOfFood2")) yield return change;

        foreach (var change in SetIfNotGame("FoodPollution_SprayRadius", MoldSprayRadius, GameSprayRadius)) yield return change;
        foreach (var change in SetIfNotGame("FoodPollution_SprayItemCost", MoldSprayCost, GameSprayCost)) yield return change;
        foreach (var change in Scale(MoldyFoodSpoilSpeed, "FoodPollution_ItemDecayScale")) yield return change;
        foreach (var change in Scale(MoldSpreadInterval, "FoodPollution_SpreadIntervalHours", "FoodPollution_SpreadIntervalMin")) yield return change;
        foreach (var change in SetIfNotGame("FoodPollution_SpreadItemsMax", MoldSpreadItems, GameSpreadItems)) yield return change;
        foreach (var change in SetIfNotGame("FoodPollution_SeedCountDay1", MoldSeedsDay1, GameSeedsDay1)) yield return change;

        foreach (var change in ScaleChance(RatVisitChance, "RatVisit_BaseChance", "RatVisit_StepChance", "RatVisit_MaxChance")) yield return change;
        foreach (var change in Scale(RatEscapeTime, "RatVisit_EscapeSeconds")) yield return change;
    }

    /// <summary>The sickness chances to change in the Config_Effect table, by row ID. Each change's key is the row ID as text.</summary>
    public IReadOnlyDictionary<int, TableChange> EffectChanges()
    {
        var changes = new Dictionary<int, TableChange>();

        if (WindChillChance != 1f) changes[WindChillEffect] = new ScaleChanceBy(EffectKey(WindChillEffect), WindChillChance);
        if (FeverChance != 1f) changes[FeverEffect] = new ScaleChanceBy(EffectKey(FeverEffect), FeverChance);

        return changes;
    }

    public static string EffectKey(int effectId) => effectId.ToString(CultureInfo.InvariantCulture);

    private static IEnumerable<TableChange> Scale(float factor, params string[] keys)
    {
        if (factor == 1f) yield break;

        foreach (var key in keys) yield return new ScaleBy(key, factor);
    }

    private static IEnumerable<TableChange> ScaleChance(float factor, params string[] keys)
    {
        if (factor == 1f) yield break;

        foreach (var key in keys) yield return new ScaleChanceBy(key, factor);
    }

    private static IEnumerable<TableChange> SetIfNotGame(string key, int value, int gameValue)
    {
        if (value == gameValue) yield break;

        yield return new SetTo(key, value);
    }

    /// <summary>Multiplies a chance and keeps it between 0 and 1, so a large factor means "always", never more.</summary>
    public sealed record ScaleChanceBy(string Key, float Factor) : TableChange(Key)
    {
        public override float Apply(float gameValue) => Math.Clamp(gameValue * Factor, 0f, 1f);
    }
}
