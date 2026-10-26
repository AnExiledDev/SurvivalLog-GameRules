using System;
using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>
/// Asks the game about each tag once per check. Several opening events share an area, so the same tag would be asked
/// again and again, and every question hands the game a new copy of the tag's text. <see cref="Use"/> starts a check
/// with the game's own reader and forgets every answer; <see cref="Forget"/> drops them after the tags change.
/// </summary>
public sealed class TagMemo
{
    private readonly Dictionary<string, bool> _seen = new();
    private Func<string, bool>? _read;

    public TagMemo()
    {
        HasTag = Ask;
    }

    /// <summary>One delegate for the memo's whole life, so handing it out makes nothing new.</summary>
    public Func<string, bool> HasTag { get; }

    /// <summary>Starts a check reading through <paramref name="read"/>, or ends one with null.</summary>
    public void Use(Func<string, bool>? read)
    {
        _read = read;
        _seen.Clear();
    }

    public void Forget() => _seen.Clear();

    private bool Ask(string tag)
    {
        if (_seen.TryGetValue(tag, out var has)) return has;

        has = (_read ?? throw new InvalidOperationException("no check is running")).Invoke(tag);
        _seen[tag] = has;

        return has;
    }
}
