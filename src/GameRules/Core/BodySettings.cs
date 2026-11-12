using System.Collections.Generic;
using System.Globalization;

namespace GameRules.Core;

/// <summary>
/// The settings for skill XP, stamina, stimulants and the satiety and morale maximums. Multipliers of 1, a daily
/// limit of 0 and extra points of 0 are the game's rules and change nothing.
/// </summary>
public sealed record BodySettings(
    float SkillXp,
    float ActionStaminaCost,
    float StimulantStrength,
    int StimulantDailyLimit,
    int ExtraMaxSatiety,
    int ExtraMaxMorale)
{
    public static BodySettings Game { get; } = new(
        SkillXp: 1f,
        ActionStaminaCost: 1f,
        StimulantStrength: 1f,
        StimulantDailyLimit: 0,
        ExtraMaxSatiety: 0,
        ExtraMaxMorale: 0);

    /// <summary>
    /// The DailyMax each stimulant's item row should hold, by item id. The game gives them all 0, which means no limit,
    /// so at 0 nothing is changed and the game's 0 comes back.
    /// </summary>
    public IReadOnlyDictionary<int, TableChange> DailyLimitChanges()
    {
        var changes = new Dictionary<int, TableChange>();

        if (StimulantDailyLimit <= 0) return changes;

        foreach (var item in Stimulants.Items) changes[item] = new SetTo(DailyLimitKey(item), StimulantDailyLimit);

        return changes;
    }

    public static string DailyLimitKey(int item) => "DailyMax_" + item.ToString(CultureInfo.InvariantCulture);
}
