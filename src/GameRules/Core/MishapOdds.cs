namespace GameRules.Core;

/// <summary>
/// A supported stranger's daily mishap roll succeeds below its own chance times the game's multiplier for the day, and
/// the game reads a multiplier of 0 or less as 1. The setting multiplies on top of that.
/// </summary>
public static class MishapOdds
{
    public static float Multiplier(float gameMultiplier, float setting) => (gameMultiplier <= 0f ? 1f : gameMultiplier) * setting;

    /// <summary>At 0 the roll is skipped: a multiplier of 0 would read as 1 to the game.</summary>
    public static bool SkipsRoll(float setting) => setting <= 0f;
}
