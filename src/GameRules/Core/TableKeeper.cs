using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>
/// Remembers the game's own value of every GlobalSetting the mod has touched, so a setting turned back to the game's
/// rule puts the game's value back. A table entry the mod has not seen before (the first load, or a reload that built
/// fresh entries) still holds the game's value, so that is when the game's value is read.
/// </summary>
public sealed class TableKeeper
{
    private readonly Dictionary<string, Seen> _seen = new();

    /// <summary>Every key the mod has changed at some point, so a key dropped from the changes still gets its value back.</summary>
    public IEnumerable<string> Keys => _seen.Keys;

    /// <summary>
    /// The value the key should hold, or null for a key the mod has never changed and does not change now.
    /// <paramref name="entryId"/> tells table entries apart; <paramref name="entryValue"/> is what the entry holds.
    /// </summary>
    public float? Target(string key, long entryId, float entryValue, TableChange? change)
    {
        _seen.TryGetValue(key, out var seen);

        if (seen == null && change == null) return null;

        if (seen == null || seen.EntryId != entryId)
        {
            seen = new Seen(entryId, entryValue);
            _seen[key] = seen;
        }

        return change?.Apply(seen.GameValue) ?? seen.GameValue;
    }

    /// <summary>The game's own value for a key the mod has seen, for logs.</summary>
    public float? GameValue(string key) => _seen.TryGetValue(key, out var seen) ? seen.GameValue : null;

    private sealed record Seen(long EntryId, float GameValue);
}
