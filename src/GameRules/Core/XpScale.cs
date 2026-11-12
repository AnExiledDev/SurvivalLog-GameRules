using System;
using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>
/// Scales the proficiency a skill gains. XP comes in small whole numbers, so the part a scaled gain leaves over is kept
/// for that skill and added to its next gain: at 1.5, gains of 1 give 1, 2, 1, 2 and so on, which adds up right.
/// </summary>
public sealed class XpScale
{
    /// <summary>Float maths can land a hair under a whole number (3 x 0.1 x 10), which must still count as that number.</summary>
    private const double Slack = 1e-4;

    /// <summary>The Inherit source is XP carried over from an earlier run; it was already scaled when it was earned.</summary>
    public const int InheritSource = 3;

    private readonly Dictionary<int, double> _leftOver = new();

    public float Factor { get; private set; } = 1f;

    /// <summary>A new factor starts every skill's left-over part again, so an old factor's part is never paid at the new one.</summary>
    public void Use(float factor)
    {
        if (factor == Factor) return;

        Factor = factor;
        _leftOver.Clear();
    }

    public static bool Scales(int source) => source != InheritSource;

    /// <summary>The XP a skill gets for a gain of <paramref name="exp"/>.</summary>
    public int Base(int skill, int exp)
    {
        if (Factor == 1f || exp <= 0) return exp;

        _leftOver.TryGetValue(skill, out var leftOver);

        var total = exp * (double)Factor + leftOver;
        var whole = Math.Floor(total + Slack);

        _leftOver[skill] = Math.Max(0d, total - whole);

        return (int)Math.Min(whole, int.MaxValue);
    }

    /// <summary>
    /// The one-time XP for a first discovery. The game only pays it if the discovery is new, which is not known before
    /// the call, so it is rounded on its own instead of touching the left-over part. Above 0 it never rounds down to nothing.
    /// </summary>
    public int Discovery(int exp)
    {
        if (Factor == 1f || exp <= 0) return exp;

        if (Factor <= 0f) return 0;

        return Math.Max(1, (int)Math.Min(Math.Round(exp * (double)Factor, MidpointRounding.AwayFromZero), int.MaxValue));
    }
}
