using System;
using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>
/// Which parts of the Outbreak crisis are stopped. Each story character has its own copy of the plague's events:
/// "Feeling Unwell" starts the crisis and marks the save, the outbreak itself brings the illness, "Disease Worsens"
/// makes it worse when rats were left alone, and "Epidemic Subsides" takes the illness away, clears the mark and pays
/// a reward.
/// </summary>
public sealed record PlagueBlock(bool NoPlague, bool NoWorsening)
{
    private static readonly HashSet<int> StartEvents = new() { 40600, 52600, 62600 };
    private static readonly HashSet<int> OutbreakEvents = new() { 40603, 52603, 62603 };
    private static readonly HashSet<int> WorseningEvents = new() { 40602, 52602, 62602 };
    private static readonly HashSet<int> EndEvents = new() { 40601, 52601, 62601 };

    public static PlagueBlock Game { get; } = new(NoPlague: false, NoWorsening: false);

    public bool BlocksAny => NoPlague || NoWorsening;

    /// <summary>
    /// Whether the event is stopped. The ending is stopped only when the save carries no plague, so a plague that never
    /// came pays no reward, while one already underway still ends and its illness clears. <paramref name="isPlagueUnderway"/>
    /// is asked only for the ending.
    /// </summary>
    public bool BlocksEvent(int eventId, Func<bool> isPlagueUnderway)
    {
        if (WorseningEvents.Contains(eventId)) return NoPlague || NoWorsening;

        if (StartEvents.Contains(eventId) || OutbreakEvents.Contains(eventId)) return NoPlague;

        if (EndEvents.Contains(eventId)) return NoPlague && !isPlagueUnderway();

        return false;
    }
}
