using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>
/// The settings for power, the house, cold weather, stray zombies and zombie horde waves. Multipliers of 1, delays of
/// 0, switches that are off and the game's own day, hour and count values change nothing.
/// </summary>
public sealed record HouseZombieSettings(
    float DevicePowerUse,
    int PowerCutDelay,
    bool NoPowerCut,
    bool NoStormLeaks,
    float ColdWaveStrength,
    int ColdResistBonus,
    bool NoStrayZombies,
    int StrayZombieStartDay,
    int StrayZombieCooldownHours,
    int StrayZombieMaxPerDay,
    int StrayZombieMaxAlive,
    float StrayZombieLeaderChance,
    float StrayZombieLoot,
    bool NoAdaptiveWaves,
    float WaveLoot)
{
    /// <summary>The day stray zombies start coming on, as the player counts days (the first day is 1).</summary>
    public const int GameStrayZombieStartDay = 7;

    /// <summary>The game hours after a stray zombie visit before the next can come.</summary>
    public const int GameStrayZombieCooldownHours = 4;

    /// <summary>The most stray zombie visits in one day.</summary>
    public const int GameStrayZombieMaxPerDay = 3;

    /// <summary>The most stray zombies around the house at once.</summary>
    public const int GameStrayZombieMaxAlive = 3;

    public static HouseZombieSettings Game { get; } = new(
        DevicePowerUse: 1f,
        PowerCutDelay: 0,
        NoPowerCut: false,
        NoStormLeaks: false,
        ColdWaveStrength: 1f,
        ColdResistBonus: 0,
        NoStrayZombies: false,
        StrayZombieStartDay: GameStrayZombieStartDay,
        StrayZombieCooldownHours: GameStrayZombieCooldownHours,
        StrayZombieMaxPerDay: GameStrayZombieMaxPerDay,
        StrayZombieMaxAlive: GameStrayZombieMaxAlive,
        StrayZombieLeaderChance: 1f,
        StrayZombieLoot: 1f,
        NoAdaptiveWaves: false,
        WaveLoot: 1f);

    public HouseEventBlock Events => new(PowerCutDelay, NoPowerCut, NoStormLeaks);

    /// <summary>Whether a killed stray zombie's loot needs rolling more or fewer times than once; none at all is a table change.</summary>
    public bool RerollsStrayLoot => StrayZombieLoot > 0f && StrayZombieLoot != 1f;

    /// <summary>The GlobalSetting values these settings change. Stray zombie values are read by name, each time they are used.</summary>
    public IEnumerable<TableChange> TableChanges()
    {
        if (NoStrayZombies) yield return new SetTo("StrayZombieEnabled", 0f);
        if (StrayZombieStartDay != GameStrayZombieStartDay) yield return new SetTo("StrayZombieMinDay", StrayZombieStartDay);
        if (StrayZombieCooldownHours != GameStrayZombieCooldownHours) yield return new SetTo("StrayZombieCooldownHours", StrayZombieCooldownHours);
        if (StrayZombieMaxPerDay != GameStrayZombieMaxPerDay) yield return new SetTo("StrayZombieMaxPerDay", StrayZombieMaxPerDay);
        if (StrayZombieMaxAlive != GameStrayZombieMaxAlive) yield return new SetTo("StrayZombieMaxAlive", StrayZombieMaxAlive);
        if (StrayZombieLoot <= 0f) yield return new SetTo("StrayZombieLootGroupId", 0f);
        if (NoAdaptiveWaves) yield return new SetTo("SiegeElastic_Enable", 0f);

        foreach (var change in WaveLootScale.Changes(WaveLoot)) yield return change;
    }
}
