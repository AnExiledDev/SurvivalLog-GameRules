using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>
/// Which of the game's night visitors are turned away. The door-smasher is the angry neighbour's "Midnight Door
/// Pounding"; the debt collector is the loan shark's visits, from the first knock through the revenge and stalking
/// nights to the feeding task.
/// </summary>
public sealed record VisitorBlock(bool NoDoorSmasher, bool NoDebtCollector)
{
    /// <summary>"Midnight Door Pounding" and its three endings.</summary>
    private static readonly HashSet<int> DoorSmasherEvents = new() { 20225, 20226, 20227, 20228 };

    /// <summary>The three "Midnight Visitors" tiers and their choices, the feeding task and its results, the revenge and stalking nights.</summary>
    private static readonly HashSet<int> DebtCollectorEvents = new()
    {
        20210, 20211, 20212, 20213, 20214, 20215, 20216, 20217, 20218, 20219, 20220, 20221,
        20223, 20224, 20233, 20234, 20235, 20236, 20237, 20238, 20239, 20240, 20241, 20242, 20243,
    };

    /// <summary>The loan shark's "I'll come by in a few days" warning texts, one per loan size.</summary>
    private static readonly HashSet<int> DebtCollectorTexts = new() { 20222, 20223, 20224 };

    public static VisitorBlock Game { get; } = new(NoDoorSmasher: false, NoDebtCollector: false);

    public bool BlocksAny => NoDoorSmasher || NoDebtCollector;

    public bool BlocksEvent(int eventId) =>
        (NoDoorSmasher && DoorSmasherEvents.Contains(eventId)) || (NoDebtCollector && DebtCollectorEvents.Contains(eventId));

    public bool BlocksText(int textId) => NoDebtCollector && DebtCollectorTexts.Contains(textId);
}
