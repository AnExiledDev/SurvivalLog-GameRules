using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace GameRules.Adapters;

/// <summary>
/// The Unity object that hands frames to the mod's checks. Each check runs on its own, so one that fails cannot stop
/// the others. Each distinct failure is logged once, never rethrown into the game; a failure that repeats every frame
/// would otherwise flood the log.
/// </summary>
public sealed class RulesTicker : MonoBehaviour
{
    private const int MaxReportedFailures = 20;

    internal static readonly List<Action<float>> Checks = new();
    internal static Action<string> Log = _ => { };
    private static readonly HashSet<string> ReportedFailures = new();

    public RulesTicker(IntPtr pointer) : base(pointer)
    {
    }

    private int _nextCheck;

    /// <summary>The checks only compare times with each other, so any steady clock will do.</summary>
    private static readonly long Start = Stopwatch.GetTimestamp();

    /// <summary>
    /// One check per frame, in turn. Each check runs on its own 1-2 second timer, and all of them started together,
    /// so running them all every frame put every table read on the same frame. The time comes from .NET's own clock:
    /// Unity's Time.realtimeSinceStartup through interop boxes its answer on every frame.
    /// </summary>
    private void Update()
    {
        if (Checks.Count == 0) return;

        _nextCheck = (_nextCheck + 1) % Checks.Count;

        try
        {
            Checks[_nextCheck]((float)((Stopwatch.GetTimestamp() - Start) / (double)Stopwatch.Frequency));
        }
        catch (Exception ex)
        {
            ReportFailure(ex);
        }
    }

    private static void ReportFailure(Exception ex)
    {
        if (ReportedFailures.Count >= MaxReportedFailures || !ReportedFailures.Add(ex.GetType().FullName + ": " + ex.Message)) return;

        Log("update failed (each distinct failure reported once): " + ex);

        if (ReportedFailures.Count == MaxReportedFailures) Log($"{MaxReportedFailures} distinct update failures reported; further ones are not logged");
    }
}
