using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>Which house events are held back: the heavy-rain leaks.</summary>
public sealed record HouseEventBlock(bool NoStormLeaks)
{
    /// <summary>"Heavy Rain Strikes" for each character and the shared one. Every choice in them costs something.</summary>
    private static readonly HashSet<int> StormLeakEvents = new() { 10010, 40700, 52700, 62700 };

    public static HouseEventBlock Game { get; } = new(NoStormLeaks: false);

    public bool BlocksAny => NoStormLeaks;

    public bool BlocksEvent(int eventId) => NoStormLeaks && StormLeakEvents.Contains(eventId);
}
