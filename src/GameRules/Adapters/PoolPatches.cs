using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using HarmonyLib;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Puts the heat pool into the game's zone climate. Every reader of a zone's temperature (the thermal display, the
/// comfort level your character feels, the warm-room tags, plants) calls EnvAreaHelper.GetAreaTemperature, which adds
/// SumDeviceHeat for the zones its config lets take device heat and then the talent bonuses and the temperature cap.
/// So pooling inside SumDeviceHeat keeps all of that in the game's order; the Terrace never reaches SumDeviceHeat, so
/// its pool heat is added after GetAreaTemperature. IsAreaAcPowered (behind the player-room AC condition and the
/// thermal display) is pooled too, so they agree with the temperature. Every answer comes from
/// <see cref="PooledClimate"/>; the hooks only hand it the game's own answer. Static because Harmony patches are.
/// </summary>
internal static class PoolPatches
{
    private static PooledClimate _climate = new(new HeatPool(new PoolSettings(false, false, false)), new GameClimate());
    private static Action<string> _log = _ => { };
    private static readonly HashSet<string> LoggedOnce = new();

    public static void Apply(Harmony harmony, PooledClimate climate, Action<string> log)
    {
        _climate = climate;
        _log = log;

        if (!climate.Pool.IsActive)
        {
            log("no zone joins Indoor, every zone keeps the game's device heat");

            return;
        }

        Hook(harmony, nameof(EnvAreaHelper.SumDeviceHeat), nameof(PoolDeviceHeat), "zones sharing heat add up their device heat");
        Hook(harmony, nameof(EnvAreaHelper.GetAreaTemperature), nameof(AddExtraWarmth), "sharing zones the game gives no device heat get the shared heat");
        Hook(harmony, nameof(EnvAreaHelper.IsAreaAcPowered), nameof(PoolAcPowered), "an AC running in one sharing zone counts for every sharing zone");
        log($"device heat is shared by {climate.Pool.Describe()}");
    }

    private static void Hook(Harmony harmony, string target, string postfix, string what)
    {
        try
        {
            var method = AccessTools.Method(typeof(EnvAreaHelper), target, new[] { typeof(int) })
                         ?? throw new MissingMethodException($"EnvAreaHelper.{target}(int)");

            harmony.Patch(method, postfix: new HarmonyMethod(typeof(PoolPatches), postfix));
            _log(what);
        }
        catch (Exception ex)
        {
            _log($"could not hook EnvAreaHelper.{target}, it keeps the game's answers: {ex.Message}");
        }
    }

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code.</summary>
    private static void PoolDeviceHeat(int areaId, ref int __result)
    {
        if (!ShouldPool(areaId)) return;

        try
        {
            var gameHeat = __result;

            __result = _climate.DeviceHeat(areaId, gameHeat);

            if (__result != gameHeat) LogOnce("heat", $"first shared device heat: {Zones.Name(areaId)} {gameHeat} in the game, {__result} shared");
        }
        catch (Exception ex)
        {
            LogOnce("heat failure", $"sharing device heat failed, zones keep the game's heat (logged once): {ex}");
        }
    }

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code.</summary>
    private static void AddExtraWarmth(int areaId, ref int __result)
    {
        if (!ShouldPool(areaId)) return;

        try
        {
            var gameTemperature = __result;

            __result = _climate.Temperature(areaId, gameTemperature);

            if (__result != gameTemperature) LogOnce("warmth", $"first shared warmth for a zone the game gives no device heat: {Zones.Name(areaId)} {gameTemperature} in the game, {__result} with the pool");
        }
        catch (Exception ex)
        {
            LogOnce("warmth failure", $"adding shared warmth failed, zones keep the game's temperature (logged once): {ex}");
        }
    }

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code.</summary>
    private static void PoolAcPowered(int areaId, ref bool __result)
    {
        if (__result || !ShouldPool(areaId)) return;

        try
        {
            __result = _climate.IsAcPowered(areaId, gameSaysPowered: false);

            if (__result) LogOnce("ac", $"first shared AC: {Zones.Name(areaId)} has no AC of its own, an AC in a zone sharing heat with it counts");
        }
        catch (Exception ex)
        {
            LogOnce("ac failure", $"sharing the AC check failed, zones keep the game's answer (logged once): {ex}");
        }
    }

    /// <summary>
    /// False for the mod's own reads of the game, for zones outside the pool, and before the world exists: the game's
    /// methods read BattleLogicWorld and ConfigManager through get_Instance, which would build a world the game has not
    /// made yet.
    /// </summary>
    private static bool ShouldPool(int areaId)
    {
        if (GameClimate.IsReadingGame || !_climate.Pool.Contains(areaId)) return false;

        try
        {
            return GameSingleton.Existing<BattleLogicWorld>() != null && GameSingleton.Existing<ConfigManager>() != null;
        }
        catch (Exception ex)
        {
            LogOnce("world failure", $"checking the game world failed, zones keep the game's answers (logged once): {ex}");

            return false;
        }
    }

    private static void LogOnce(string key, string message)
    {
        lock (LoggedOnce)
        {
            if (!LoggedOnce.Add(key)) return;
        }

        _log(message);
    }
}
