namespace GameRules.Core;

/// <summary>Which zones share the house's device heat with Indoor. Cold storage has no switch: it never joins.</summary>
public sealed record PoolSettings(bool IndoorAndSunroom, bool Terrace, bool Basement)
{
    /// <summary>The game's own rules: every zone keeps its own device heat.</summary>
    public static PoolSettings Default { get; } = new(IndoorAndSunroom: false, Terrace: false, Basement: false);
}
