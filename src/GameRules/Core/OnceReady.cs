using System;

namespace GameRules.Core;

/// <summary>
/// Runs a job once, at the first check where the game is ready for it. It is marked done before it runs: a job that
/// throws only warms something up, so trying it again at every check would repeat the failure for nothing.
/// </summary>
public sealed class OnceReady
{
    private readonly Func<bool> _isReady;
    private readonly Action _job;
    private bool _hasRun;

    public OnceReady(Func<bool> isReady, Action job)
    {
        _isReady = isReady;
        _job = job;
    }

    /// <summary>A ticker check; the time is not needed.</summary>
    public void Tick(float _)
    {
        if (_hasRun || !_isReady()) return;

        _hasRun = true;
        _job();
    }
}
