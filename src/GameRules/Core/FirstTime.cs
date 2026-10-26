using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>
/// Whether an id comes up for the first time. A held-back event is offered again at every check, so its once-only log
/// line is only built the first time rather than built and thrown away each check.
/// </summary>
public sealed class FirstTime
{
    private readonly HashSet<int> _seen = new();

    public bool Of(int id)
    {
        lock (_seen) return _seen.Add(id);
    }
}
