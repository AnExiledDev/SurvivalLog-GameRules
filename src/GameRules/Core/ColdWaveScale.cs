using System;

namespace GameRules.Core;

/// <summary>
/// How cold the weather is (the game's ColdValue: 0 on mild days, 1 to 3 in cold waves, frosty days and cold rain)
/// and how many comfort tiers the player's own cold resistance adds (the game caps comfort at 3).
/// </summary>
public static class ColdWaveScale
{
    /// <summary>The weather's cold times <paramref name="strength"/>, rounded half up and never below 0.</summary>
    public static int Cold(int gameCold, float strength)
    {
        if (strength == 1f) return gameCold;

        return Math.Max(0, (int)Math.Round(gameCold * strength, MidpointRounding.AwayFromZero));
    }

    /// <summary>The player's personal cold resistance in tiers, with the bonus tiers added.</summary>
    public static float ResistTiers(float gameTiers, int bonusTiers) => bonusTiers > 0 ? gameTiers + bonusTiers : gameTiers;
}
