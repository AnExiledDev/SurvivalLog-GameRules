using System;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;
using Il2CppInterop.Runtime;

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

    /// <summary>
    /// Read from the table itself rather than through Get_Config_EnvArea, which writes an error to the game's log for
    /// every zone it has no row for.
    /// </summary>
    public bool? TakesDeviceHeat(int zone)
    {
        var rows = GameSingleton.Existing<ConfigManager>()?._Config_EnvArea_Dict;

        if (rows == null || !rows.TryGetValue(zone, out var row) || row == null) return null;

        return row._DeviceHeat_k__BackingField;
    }

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

    /// <summary>
    /// The world, and the zone table rather than the config manager: loading the configs again keeps the manager and
    /// gives it new tables.
    /// </summary>
    public LoadedGame CurrentGame() =>
        new((long)GameSingleton.Pointer<BattleLogicWorld>(), (long)ZoneTable.Pointer(GameSingleton.Pointer<ConfigManager>()));

    /// <summary>No lambda here: these run dozens of times a frame, and a closure is a new object each call.</summary>
    private static bool StartReading()
    {
        var wasReading = _isReadingGame;

        _isReadingGame = true;

        return wasReading;
    }

    /// <summary>
    /// The config manager's zone table, read straight from its field: through interop the read makes a new wrapper
    /// each time. The explicit static constructor keeps the lookup at the first read in a game frame.
    /// </summary>
    private static class ZoneTable
    {
        private static readonly int Offset;

        static ZoneTable()
        {
            try
            {
                var field = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<ConfigManager>.NativeClassPtr, "_Config_EnvArea_Dict");

                Offset = field == IntPtr.Zero ? 0 : (int)IL2CPP.il2cpp_field_get_offset(field);
            }
            catch (Exception)
            {
                Offset = 0;
            }
        }

        public static unsafe IntPtr Pointer(IntPtr configs)
        {
            if (configs == IntPtr.Zero) return IntPtr.Zero;

            if (Offset == 0) return GameSingleton.Existing<ConfigManager>()?._Config_EnvArea_Dict?.Pointer ?? IntPtr.Zero;

            return *(IntPtr*)(configs + Offset);
        }
    }
}
