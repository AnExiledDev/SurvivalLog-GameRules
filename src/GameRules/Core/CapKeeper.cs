namespace GameRules.Core;

/// <summary>
/// Tracks the points the mod has added to one of the player's maximums. The game adds and takes away other bonuses
/// on the same value (buffs put theirs on and take it back off), so the mod only ever adds the difference between
/// what it wants now and what it already added, and takes all of it back when the setting returns to 0.
/// </summary>
public sealed class CapKeeper
{
    private long _character;

    /// <summary>The points the mod has added to the current character's maximum.</summary>
    public int Added { get; private set; }

    /// <summary>
    /// The points to add now so the character holds <paramref name="wanted"/>, counted as added. A different character
    /// (a new game or a load) starts with nothing from the mod.
    /// </summary>
    public int Next(long character, int wanted)
    {
        if (character != _character)
        {
            _character = character;
            Added = 0;
        }

        var change = wanted - Added;

        Added = wanted;

        return change;
    }

    /// <summary>The game cleared this character's bonuses (it reuses the object for a new character), so the mod's points are gone too.</summary>
    public void Forget(long character)
    {
        if (character == _character) Added = 0;
    }
}
