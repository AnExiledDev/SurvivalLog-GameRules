using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>
/// Which house events are held back: the city power cut, until later or for good, and the heavy-rain leaks. The cut
/// is one story event per character ("Power Cut Begins"), offered from game day 14. Holding it back is safe because
/// the game offers it again at every check until it happens; a cut that already happened is not undone.
/// </summary>
public sealed record HouseEventBlock(int PowerCutDelay, bool NoPowerCut, bool NoStormLeaks)
{
    /// <summary>The first game day (counted from 0, as the event table does) the game offers the power cut on.</summary>
    public const int GamePowerCutDay = 14;

    /// <summary>"Power Cut Begins" for each of the three characters.</summary>
    private static readonly HashSet<int> PowerCutEvents = new() { 40019, 52019, 62019 };

    /// <summary>"Heavy Rain Strikes" for each character and the shared one. Every choice in them costs something.</summary>
    private static readonly HashSet<int> StormLeakEvents = new() { 10010, 40700, 52700, 62700 };

    public static HouseEventBlock Game { get; } = new(PowerCutDelay: 0, NoPowerCut: false, NoStormLeaks: false);

    public bool BlocksAny => PowerCutDelay > 0 || NoPowerCut || NoStormLeaks;

    /// <summary>Whether the answer for this event depends on the day; for every other event the day need not be read.</summary>
    public static bool ReadsDay(int eventId) => PowerCutEvents.Contains(eventId);

    /// <summary>
    /// Whether the event is held back on <paramref name="gameDay"/> (counted from 0). An unknown day lets a delayed
    /// power cut through, so a failed read keeps the game's rule.
    /// </summary>
    public bool BlocksEvent(int eventId, int? gameDay)
    {
        if (NoStormLeaks && StormLeakEvents.Contains(eventId)) return true;

        if (!PowerCutEvents.Contains(eventId)) return false;

        if (NoPowerCut) return true;

        return PowerCutDelay > 0 && gameDay is { } day && day < GamePowerCutDay + PowerCutDelay;
    }
}
