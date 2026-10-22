using System;
using System.Collections.Generic;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;
using HarmonyLib;

namespace GameRules.Adapters;

/// <summary>
/// Turns Blackout panic off. PowerManager.SyncBlackoutPanicBuff is where the game gives the panic buff (at night
/// with the power off) and takes it away again; it runs every ten in-game minutes and on every power change. With the
/// switch on, it is skipped and the buff is taken away instead, so a panic already running clears at the next check.
/// The target was checked in GameAssembly.dll: no relative jump or call in its first 14 bytes. Static because Harmony
/// patches are.
/// </summary>
internal static class BlackoutPanicPatches
{
    /// <summary>The Config_Buff ID of Blackout panic: morale drops every two hours until it is taken away.</summary>
    private const int BlackoutPanicBuff = 30002;

    private static readonly HashSet<string> LoggedOnce = new();
    private static bool _isPanicOff;
    private static bool _isHooked;
    private static Action<string> _log = _ => { };

    /// <summary>
    /// Called on the main thread at start and after each change. The hook goes in the first time panic is turned off and
    /// stays in; turned back on, blackouts bring panic as in the game again.
    /// </summary>
    public static void Use(Harmony harmony, bool noBlackoutPanic, Action<string> log)
    {
        _log = log;
        _isPanicOff = noBlackoutPanic;

        if (!noBlackoutPanic || _isHooked) return;

        _isHooked = true;

        Hook(harmony);
    }

    private static void Hook(Harmony harmony)
    {
        try
        {
            var method = AccessTools.Method(typeof(PowerManager), "SyncBlackoutPanicBuff", Type.EmptyTypes)
                ?? throw new MissingMethodException($"{nameof(PowerManager)}.SyncBlackoutPanicBuff");

            harmony.Patch(method, prefix: new HarmonyMethod(typeof(BlackoutPanicPatches), nameof(ClearPanic)));
        }
        catch (Exception ex)
        {
            _log($"{HookFailure.Prefix} {nameof(PowerManager)}.SyncBlackoutPanicBuff, blackouts bring panic as in the game: {ex.Message}");
        }
    }

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code, so a failure keeps the game's rule.</summary>
    private static bool ClearPanic(PowerManager __instance)
    {
        try
        {
            if (!_isPanicOff) return true;

            if (__instance == null || __instance._IsLocked_k__BackingField) return false;

            var role = GameSingleton.Existing<BattleLogicWorld>()?._AgentManager?.GetLeadingRole();
            var buffs = role == null ? null : AgentTools.GetAgentComponent<BuffComponent>(role);

            if (buffs == null || !buffs.IsBuffConfigExist(BlackoutPanicBuff)) return false;

            buffs.RemoveBuff(BlackoutPanicBuff);
            LogOnce("cleared", "took away a Blackout panic that was already running (logged once)");

            return false;
        }
        catch (Exception ex)
        {
            LogOnce("panic failure", $"checking Blackout panic failed, it works as in the game (logged once): {ex}");

            return true;
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
