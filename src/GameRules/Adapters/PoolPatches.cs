using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
    private static Action<string> _fileLog = _ => { };
    private static readonly HashSet<string> LoggedOnce = new();
    private static bool _isHooked;

    // The hooks run dozens of times a frame, so each first-time line is only built while it is still unlogged.
    private static bool _hasLoggedHeat;
    private static bool _hasLoggedWarmth;
    private static bool _hasLoggedAc;

    /// <summary>
    /// Called on the main thread at start and after each change. The hooks go in the first time a zone joins Indoor and
    /// stay in; a zone that leaves keeps the game's answers from the next reading on. <paramref name="fileLog"/> writes
    /// only to the mod's own file, from a worker: the warm-up's thread uses it, and so do the hooks' first-time lines,
    /// so a hook never writes to the BepInEx console inside the game's own call.
    /// </summary>
    public static void Use(Harmony harmony, PooledClimate climate, Action<string> log, Action<string> fileLog)
    {
        _climate = climate;
        _log = log;
        _fileLog = fileLog;

        if (!climate.Pool.IsActive)
        {
            log("no zone joins Indoor, every zone keeps the game's device heat");

            return;
        }

        if (!_isHooked)
        {
            _isHooked = true;

            Hook(harmony, nameof(EnvAreaHelper.SumDeviceHeat), nameof(PoolDeviceHeat), "zones sharing heat add up their device heat");
            Hook(harmony, nameof(EnvAreaHelper.GetAreaTemperature), nameof(AddExtraWarmth), "sharing zones the game gives no device heat get the shared heat");
            Hook(harmony, nameof(EnvAreaHelper.IsAreaAcPowered), nameof(PoolAcPowered), "an AC running in one sharing zone counts for every sharing zone");
            WarmUpInBackground(fileLog);
            RulesTicker.Checks.Add(new OnceReady(IsWorldReady, WarmGameReads).Tick);
        }

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

    /// <summary>
    /// The first temperature read of a session would otherwise compile the pool's code on the game's frame. The pool's
    /// own code runs once on throwaway objects, and the hooks and the game reads are compiled without being run, so
    /// nothing here touches the game. A failure only costs that first read its head start, and is noted in the mod's
    /// file only: the BepInEx console is left to the game's thread.
    /// </summary>
    private static void WarmUpInBackground(Action<string> fileLog) =>
        Task.Run(() =>
        {
            try
            {
                PoolWarmUp.Run();
                Prepare(typeof(PoolPatches), nameof(PoolDeviceHeat), nameof(AddExtraWarmth), nameof(PoolAcPowered), nameof(ShouldPool), nameof(LogOnce));
                Prepare(typeof(GameClimate), nameof(GameClimate.DeviceHeatIn), nameof(GameClimate.TakesDeviceHeat), nameof(GameClimate.IsAcPoweredIn), nameof(GameClimate.CurrentGame));
                Prepare(typeof(Zones), nameof(Zones.Name));
            }
            catch (Exception ex)
            {
                fileLog("warm-up failed, the first temperature read may take a moment longer: " + ex.Message);
            }
        });

    private static bool IsWorldReady() =>
        GameSingleton.IsCreated<BattleLogicWorld>() && GameSingleton.IsCreated<ConfigManager>();

    /// <summary>
    /// The worker can compile the code but cannot touch the game, so the first pooled answer of a session would still
    /// be the first read of the zone table and of each zone's heat and AC through interop. These run once on the main
    /// thread as soon as the world exists, while the save loads, so that first read is not paid inside a temperature
    /// the game asked for. They go through a GameClimate of their own: the hooks' per-frame copy must not keep an
    /// answer read from a half-loaded world. Read-only, and a failure only costs the head start.
    /// </summary>
    private static void WarmGameReads()
    {
        try
        {
            var game = new GameClimate();

            game.CurrentGame();

            foreach (var zone in _climate.Pool.Members)
            {
                game.TakesDeviceHeat(zone);
                game.DeviceHeatIn(zone);
                game.IsAcPoweredIn(zone);
            }
        }
        catch (Exception ex)
        {
            _fileLog("warming up the zone reads failed, the first shared temperature may take a moment longer: " + ex.Message);
        }
    }

    private static void Prepare(Type type, params string[] names)
    {
        foreach (var name in names)
        {
            var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         ?? throw new MissingMethodException(type.Name, name);

            RuntimeHelpers.PrepareMethod(method.MethodHandle);
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

            if (__result != gameHeat && !_hasLoggedHeat)
            {
                _hasLoggedHeat = true;
                LogOnce("heat", $"first shared device heat: {Zones.Name(areaId)} {gameHeat} in the game, {__result} shared", _fileLog);
            }
        }
        catch (Exception ex)
        {
            LogOnce("heat failure", $"sharing device heat failed, zones keep the game's heat (logged once): {ex}", _log);
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

            if (__result != gameTemperature && !_hasLoggedWarmth)
            {
                _hasLoggedWarmth = true;
                LogOnce("warmth", $"first shared warmth for a zone the game gives no device heat: {Zones.Name(areaId)} {gameTemperature} in the game, {__result} with the pool", _fileLog);
            }
        }
        catch (Exception ex)
        {
            LogOnce("warmth failure", $"adding shared warmth failed, zones keep the game's temperature (logged once): {ex}", _log);
        }
    }

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code.</summary>
    private static void PoolAcPowered(int areaId, ref bool __result)
    {
        if (__result || !ShouldPool(areaId)) return;

        try
        {
            __result = _climate.IsAcPowered(areaId, gameSaysPowered: false);

            if (__result && !_hasLoggedAc)
            {
                _hasLoggedAc = true;
                LogOnce("ac", $"first shared AC: {Zones.Name(areaId)} has no AC of its own, an AC in a zone sharing heat with it counts", _fileLog);
            }
        }
        catch (Exception ex)
        {
            LogOnce("ac failure", $"sharing the AC check failed, zones keep the game's answer (logged once): {ex}", _log);
        }
    }

    /// <summary>
    /// False for the mod's own reads of the game, for zones outside the pool, and before the world exists: the game's
    /// methods read BattleLogicWorld and ConfigManager through get_Instance, which would build a world the game has not
    /// made yet. Checked without building a wrapper, since this runs on every hooked call.
    /// </summary>
    private static bool ShouldPool(int areaId)
    {
        if (GameClimate.IsReadingGame || !_climate.Pool.Contains(areaId)) return false;

        try
        {
            return GameSingleton.IsCreated<BattleLogicWorld>() && GameSingleton.IsCreated<ConfigManager>();
        }
        catch (Exception ex)
        {
            LogOnce("world failure", $"checking the game world failed, zones keep the game's answers (logged once): {ex}", _log);

            return false;
        }
    }

    /// <summary>
    /// A first-time line goes to the mod's file only (<see cref="_fileLog"/>), so the game's call never waits on the
    /// BepInEx console; a failure goes to both (<see cref="_log"/>), where it is seen.
    /// </summary>
    private static void LogOnce(string key, string message, Action<string> write)
    {
        lock (LoggedOnce)
        {
            if (!LoggedOnce.Add(key)) return;
        }

        write(message);
    }
}
