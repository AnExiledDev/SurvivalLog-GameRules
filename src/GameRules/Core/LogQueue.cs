using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace GameRules.Core;

/// <summary>
/// Hands the mod's log lines to a worker that appends them to its file, so a disk write never holds up the game's
/// frame. One write runs at a time and takes every line waiting, in the order they came; a burst of lines is one
/// write. A write that fails drops the lines it held (the BepInEx log still has them) and never stops later ones.
/// </summary>
public sealed class LogQueue
{
    private readonly ConcurrentQueue<(DateTime Time, string Message)> _waiting = new();
    private readonly Action<string> _append;
    private readonly Action<Action> _schedule;
    private readonly Action _drain;
    private readonly StringBuilder _text = new();
    private int _isDraining;

    /// <param name="append">Appends text to the file; runs on the worker.</param>
    /// <param name="schedule">Runs an action on a worker.</param>
    public LogQueue(Action<string> append, Action<Action> schedule)
    {
        _append = append;
        _schedule = schedule;
        _drain = Drain;
    }

    /// <summary>Stamped now, written later; formatting waits for the worker too.</summary>
    public void Add(DateTime time, string message)
    {
        _waiting.Enqueue((time, message));

        if (Interlocked.CompareExchange(ref _isDraining, 1, 0) != 0) return;

        try
        {
            _schedule(_drain);
        }
        catch (Exception)
        {
            // No worker this time; the next line tries again and takes this one with it.
            Volatile.Write(ref _isDraining, 0);
        }
    }

    private void Drain()
    {
        while (true)
        {
            while (_waiting.TryDequeue(out var line))
            {
                _text.Append(line.Time.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append(' ').Append(line.Message).Append(Environment.NewLine);
            }

            try
            {
                if (_text.Length > 0) _append(_text.ToString());
            }
            catch (Exception)
            {
                // A locked or read-only file costs these lines in the file copy, never the plugin.
            }

            _text.Clear();
            Volatile.Write(ref _isDraining, 0);

            // A line added after the last dequeue but before the flag cleared found the drain still running.
            if (_waiting.IsEmpty || Interlocked.CompareExchange(ref _isDraining, 1, 0) != 0) return;
        }
    }
}
