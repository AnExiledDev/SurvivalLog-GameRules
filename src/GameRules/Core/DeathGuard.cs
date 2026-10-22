namespace GameRules.Core;

/// <summary>
/// Keeps the neighbour or companion alive. The game's hourly survival check returns true only when she died that hour,
/// and the rest of the hour (rescue progress, gifts, crises) is skipped after a death. When the guard stops a death,
/// the check reports no death so the hour goes on as usual.
/// </summary>
public sealed class DeathGuard
{
    private bool _isNeverDie;
    private bool _hasStoppedDeath;

    public void Use(bool neverDie) => _isNeverDie = neverDie;

    public void CheckStarts() => _hasStoppedDeath = false;

    /// <summary>Whether the game may go on with a death it is about to carry out.</summary>
    public bool AllowsDeath()
    {
        if (!_isNeverDie) return true;

        _hasStoppedDeath = true;

        return false;
    }

    public bool CheckResult(bool gameResult) => !_hasStoppedDeath && gameResult;
}
