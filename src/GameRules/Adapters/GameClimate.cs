using System;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Reads the game's own, unpooled answers. The calls below go through the very methods the mod hooks, so each sets
/// <see cref="IsReadingGame"/> and the hooks leave those reads alone instead of pooling them again.
/// </summary>
internal sealed class GameClimate : IGameClimate
{
    [ThreadStatic] private static bool _isReadingGame;

    public static bool IsReadingGame => _isReadingGame;

    public int DeviceHeatIn(int zone)
    {
        var wasReading = StartReading();

        try
        {
            return EnvAreaHelper.SumDeviceHeat(zone);
        }
        finally
        {
            _isReadingGame = wasReading;
        }
    }

    public bool? TakesDeviceHeat(int zone) => GameSingleton.Existing<ConfigManager>()?.Get_Config_EnvArea(zone)?.DeviceHeat;

    public bool IsAcPoweredIn(int zone)
    {
        var wasReading = StartReading();

        try
        {
            return EnvAreaHelper.IsAreaAcPowered(zone);
        }
        finally
        {
            _isReadingGame = wasReading;
        }
    }

    /// <summary>No lambda here: these run dozens of times a frame, and a closure is a new object each call.</summary>
    private static bool StartReading()
    {
        var wasReading = _isReadingGame;

        _isReadingGame = true;

        return wasReading;
    }
}
