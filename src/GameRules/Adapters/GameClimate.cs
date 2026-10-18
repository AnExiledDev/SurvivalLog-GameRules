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

    public int DeviceHeatIn(int zone) => ReadGame(() => EnvAreaHelper.SumDeviceHeat(zone));

    public bool? TakesDeviceHeat(int zone) => GameSingleton.Existing<ConfigManager>()?.Get_Config_EnvArea(zone)?.DeviceHeat;

    public bool IsAcPoweredIn(int zone) => ReadGame(() => EnvAreaHelper.IsAreaAcPowered(zone));

    private static T ReadGame<T>(Func<T> read)
    {
        var wasReading = _isReadingGame;

        _isReadingGame = true;

        try
        {
            return read();
        }
        finally
        {
            _isReadingGame = wasReading;
        }
    }
}
