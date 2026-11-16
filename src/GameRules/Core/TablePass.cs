namespace GameRules.Core;

/// <summary>
/// When a table writer should look at the game's tables. The game builds its tables once, when it starts
/// (ConfigManager.Initialize, from GameEntrance.EnterMain), and nothing builds them again, so one pass that finds them
/// and writes the mod's values is enough. Until then the writer tries again every few seconds, since the plugin loads
/// before the tables do. A setting change or a load asks for one more pass.
/// </summary>
public sealed class TablePass
{
    private readonly float _retrySeconds;
    private float _nextTryAt;
    private bool _isDone;

    public TablePass(float retrySeconds)
    {
        _retrySeconds = retrySeconds;
    }

    /// <summary>True when a pass should run now. A pass that does not call <see cref="Done"/> is tried again later.</summary>
    public bool IsDue(float now)
    {
        if (_isDone || now < _nextTryAt) return false;

        _nextTryAt = now + _retrySeconds;

        return true;
    }

    /// <summary>The pass found the tables and wrote them; nothing runs again until <see cref="Redo"/>.</summary>
    public void Done() => _isDone = true;

    /// <summary>Asks for a pass at the next check: a setting changed, or a save loaded.</summary>
    public void Redo()
    {
        _isDone = false;
        _nextTryAt = 0f;
    }
}
