namespace GameRules.Core;

/// <summary>A restart cap to put in the game's field. <see cref="IsPutBack"/> when something else had changed it.</summary>
public readonly record struct RestartWrite(int Max, int GameMax, bool IsPutBack);

/// <summary>
/// Keeps the restart cap out of reach. The game allows a restart while its cap is above the restarts already used,
/// and shows one mark per restart in the cap, so a huge cap would fill the screen. Instead the cap follows the
/// restarts used: there are always as many left as the game gives a fresh run. The first cap seen is the game's own;
/// a value written later by something else is put back, never taken as the game's cap.
/// </summary>
public sealed class RestartKeeper
{
    private int? _gameMax;
    private int? _written;

    /// <param name="current">The cap field's value; 0 or less is a cap the game has not loaded yet.</param>
    /// <param name="used">Restarts used in the loaded run, or null with no run loaded.</param>
    /// <returns>The cap to write, or null when the field should stay as it is.</returns>
    public RestartWrite? Next(int current, int? used)
    {
        if (current <= 0) return null;

        var gameMax = _gameMax ??= current;

        if (used is not { } restartsUsed) return null;

        var target = Target(gameMax, restartsUsed);

        if (target == current) return null;

        var isPutBack = _written != null && current != _written;
        _written = target;

        return new RestartWrite(target, gameMax, isPutBack);
    }

    /// <summary>The cap that leaves the game's full allowance unused, whatever has been used.</summary>
    public static int Target(int gameMax, int used) => gameMax + (used > 0 ? used : 0);
}
