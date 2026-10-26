namespace GameRules.Core;

/// <summary>The game's own, unpooled answers for one zone. The pool reads the game only through this port.</summary>
public interface IGameClimate
{
    /// <summary>The heat of the running devices standing in the zone (the game's SumDeviceHeat).</summary>
    int DeviceHeatIn(int zone);

    /// <summary>
    /// Whether the zone's config lets it take device heat at all (Config_EnvArea.DeviceHeat; as shipped, false for the
    /// Terrace and Cold storage). Null when the game has no config row for the zone.
    /// </summary>
    bool? TakesDeviceHeat(int zone);

    /// <summary>Whether an AC is running in the zone itself (the game's IsAreaAcPowered).</summary>
    bool IsAcPoweredIn(int zone);

    /// <summary>Which save and which config tables are loaded. Read once a frame, so it must be free to read.</summary>
    LoadedGame CurrentGame();
}

/// <summary>
/// The loaded run and config tables, told apart by the game's own objects: loading a save makes a new world, and
/// loading the configs again makes a new config manager. Zero for one the game has not made.
/// </summary>
public readonly record struct LoadedGame(long World, long Config);
