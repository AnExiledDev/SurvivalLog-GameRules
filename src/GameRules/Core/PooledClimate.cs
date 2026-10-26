using System;

namespace GameRules.Core;

/// <summary>
/// The pooled answer for each game call the mod hooks, given the game's own answer for that call. The game warms a
/// zone in GetAreaTemperature, which adds SumDeviceHeat only for zones whose config takes device heat; so a zone that
/// takes device heat gets its pool share through <see cref="DeviceHeat"/>, and a pool member that takes none (the
/// Terrace) gets it through <see cref="Temperature"/>. Never both, so no device is counted twice.
/// </summary>
public sealed class PooledClimate
{
    private readonly IGameClimate _game;

    // Made once: the hooks run dozens of times a frame, and a method group passed as a Func is a new object each call.
    private readonly Func<int, int> _heatIn;
    private readonly Func<int, bool> _acPoweredIn;

    public PooledClimate(HeatPool pool, IGameClimate game)
    {
        Pool = pool;
        _game = game;
        _heatIn = game.DeviceHeatIn;
        _acPoweredIn = game.IsAcPoweredIn;
    }

    public HeatPool Pool { get; }

    /// <summary>The answer for SumDeviceHeat(zone), given the game's.</summary>
    public int DeviceHeat(int zone, int gameHeat) => Pool.DeviceHeatFor(zone, gameHeat, _heatIn);

    /// <summary>The answer for GetAreaTemperature(zone), given the game's (which already holds the pooled device heat).</summary>
    public int Temperature(int zone, int gameTemperature)
    {
        if (!Pool.Contains(zone)) return gameTemperature;

        var takesDeviceHeat = _game.TakesDeviceHeat(zone);

        if (takesDeviceHeat is not { } gameWarmsWithDevices) return gameTemperature;

        return gameTemperature + Pool.ExtraWarmth(zone, gameWarmsWithDevices, _heatIn);
    }

    /// <summary>The answer for IsAreaAcPowered(zone), given the game's.</summary>
    public bool IsAcPowered(int zone, bool gameSaysPowered) => Pool.IsAcPoweredFor(zone, gameSaysPowered, _acPoweredIn);
}
