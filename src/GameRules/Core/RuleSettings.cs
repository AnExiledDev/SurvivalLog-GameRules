namespace GameRules.Core;

/// <summary>The rule changes the player picked. <see cref="Game"/> is the game's own rules, so a default changes nothing.</summary>
public sealed record RuleSettings(bool NoRestartLimit, bool StopMoldSpread, float SpoilageSpeed)
{
    public static RuleSettings Game { get; } = new(NoRestartLimit: false, StopMoldSpread: false, SpoilageSpeed: 1f);

    public bool ChangesSpoilage => SpoilageSpeed != 1f;
}
