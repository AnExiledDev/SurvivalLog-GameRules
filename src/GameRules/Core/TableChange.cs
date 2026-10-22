namespace GameRules.Core;

/// <summary>A change to one value in the game's GlobalSetting table, named by its key (the part after "GlobalSetting_").</summary>
public abstract record TableChange(string Key)
{
    /// <summary>The value the game should use, given the game's own.</summary>
    public abstract float Apply(float gameValue);
}

/// <summary>Replaces the game's value.</summary>
public sealed record SetTo(string Key, float Value) : TableChange(Key)
{
    public override float Apply(float gameValue) => Value;
}

/// <summary>Multiplies the game's value, so a later game patch that retunes it keeps the player's ratio.</summary>
public sealed record ScaleBy(string Key, float Factor) : TableChange(Key)
{
    public override float Apply(float gameValue) => gameValue * Factor;
}
