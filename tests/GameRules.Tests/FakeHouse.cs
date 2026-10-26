using GameRules.Core;

namespace GameRules.Tests;

/// <summary>
/// The game's zone climate as far as device heat and ACs go: each zone has a weather offset, each running device stands
/// in one zone, and GetAreaTemperature adds a zone's SumDeviceHeat only when its config lets it take device heat (as
/// shipped, the Terrace and Cold storage do not). The "with the mod" readings run the same game calls with the mod's
/// hooks in place, each hook answering through <see cref="PooledClimate"/> exactly as the adapter does.
/// </summary>
internal sealed class FakeHouse : IGameClimate
{
    private readonly List<(int Zone, int Heat, bool IsAc)> _devices = new();

    public static readonly int[] AllZones = { Zones.Indoor, Zones.Sunroom, Zones.Terrace, Zones.ColdStorage, Zones.Basement };

    /// <summary>Config_EnvArea.DeviceHeat per zone; a zone missing here has no config row.</summary>
    public Dictionary<int, bool> DeviceHeatFlags { get; } = new()
    {
        [Zones.Indoor] = true,
        [Zones.Sunroom] = true,
        [Zones.Terrace] = false,
        [Zones.ColdStorage] = false,
        [Zones.Basement] = true,
    };

    public Dictionary<int, int> Weather { get; } = new()
    {
        [Zones.Indoor] = 5,
        [Zones.Sunroom] = 3,
        [Zones.Terrace] = -10,
        [Zones.ColdStorage] = -15,
        [Zones.Basement] = 8,
    };

    public FakeHouse Place(int zone, int heat) => Add(zone, heat, isAc: false);

    public FakeHouse PlaceAc(int zone, int heat) => Add(zone, heat, isAc: true);

    public int DeviceHeatIn(int zone) => _devices.Where(device => device.Zone == zone).Sum(device => device.Heat);

    public bool? TakesDeviceHeat(int zone) => DeviceHeatFlags.TryGetValue(zone, out var takes) ? takes : null;

    public bool IsAcPoweredIn(int zone) => _devices.Any(device => device.Zone == zone && device.IsAc);

    public LoadedGame CurrentGame() => default;

    /// <summary>The game's GetAreaTemperature without the mod.</summary>
    public int GameTemperature(int zone) => Weather[zone] + (TakesDeviceHeat(zone) == true ? DeviceHeatIn(zone) : 0);

    /// <summary>The game's GetAreaTemperature with the SumDeviceHeat and GetAreaTemperature hooks in place.</summary>
    public int Temperature(HeatPool pool, int zone)
    {
        var climate = new PooledClimate(pool, this);
        var deviceHeat = TakesDeviceHeat(zone) == true ? climate.DeviceHeat(zone, DeviceHeatIn(zone)) : 0;

        return climate.Temperature(zone, Weather[zone] + deviceHeat);
    }

    /// <summary>The game's IsAreaAcPowered with the hook in place.</summary>
    public bool IsAcPowered(HeatPool pool, int zone) => new PooledClimate(pool, this).IsAcPowered(zone, IsAcPoweredIn(zone));

    /// <summary>How much warmer the mod makes a zone than the game does.</summary>
    public int Gain(HeatPool pool, int zone) => Temperature(pool, zone) - GameTemperature(zone);

    private FakeHouse Add(int zone, int heat, bool isAc)
    {
        _devices.Add((zone, heat, isAc));

        return this;
    }
}
