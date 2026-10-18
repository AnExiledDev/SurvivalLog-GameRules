namespace GameRules.Core;

public static class SpoilageScale
{
    /// <summary>The slowest rate the game treats as real food; 0 or less marks the save as cheated.</summary>
    public const float MinGameScale = 0.002f;

    /// <summary>
    /// A room's food decay rate times the player's speed. A rate of 0 or less is the game's own "does not spoil" and
    /// stays as it is; anything else never drops below the slowest rate the game accepts.
    /// </summary>
    public static float Apply(float gameScale, float speed)
    {
        if (gameScale <= 0f) return gameScale;

        var scaled = gameScale * speed;

        return scaled < MinGameScale ? MinGameScale : scaled;
    }
}
