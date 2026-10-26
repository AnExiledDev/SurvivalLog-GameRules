using System;
using System.Collections.Generic;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;
using HarmonyLib;

namespace GameRules.Adapters;

/// <summary>
/// The hooks for the neighbour and companion deaths and for supported strangers' mishap rolls. Each target was checked
/// in GameAssembly.dll: no relative jump or call in its first 14 bytes. Static because Harmony patches are.
/// </summary>
internal static class PeoplePatches
{
    private static readonly DeathGuard NeighbourGuard = new();
    private static readonly DeathGuard CompanionGuard = new();
    private static readonly HashSet<string> LoggedOnce = new();
    private static float _mishapChance = 1f;
    private static bool _isDeathHooked;
    private static bool _isMishapHooked;
    private static bool _hasLoggedNeighbour;
    private static bool _hasLoggedCompanion;
    private static Action<string> _log = _ => { };

    /// <summary>
    /// Called on the main thread at start and after each change. Each hook goes in the first time its setting leaves the
    /// game's rule and stays in; back at the game's rule, it lets the game's own answer through.
    /// </summary>
    public static void Use(Harmony harmony, PeopleSettings settings, Action<string> log)
    {
        _log = log;
        _mishapChance = settings.MishapChance;
        NeighbourGuard.Use(settings.NeverDie);
        CompanionGuard.Use(settings.NeverDie);

        if (settings.NeverDie && !_isDeathHooked)
        {
            _isDeathHooked = true;

            HookDeaths(harmony, typeof(NeighborRescueManager), nameof(NeighbourCheck), nameof(NeighbourChecked), nameof(NeighbourDies));
            HookDeaths(harmony, typeof(GirlCompanionManager), nameof(CompanionCheck), nameof(CompanionChecked), nameof(CompanionDies));
        }

        if (settings.MishapChance != 1f && !_isMishapHooked)
        {
            _isMishapHooked = true;

            Hook(harmony, typeof(StrangerSupportManager), "StepMishap", prefix: nameof(ScaleMishapChance));
        }
    }

    private static void HookDeaths(Harmony harmony, Type manager, string checkStarts, string checkEnds, string death)
    {
        Hook(harmony, manager, "CheckSurvivalTick", prefix: checkStarts, postfix: checkEnds);
        Hook(harmony, manager, "TriggerDeath", prefix: death);
    }

    private static void Hook(Harmony harmony, Type type, string target, string? prefix = null, string? postfix = null)
    {
        try
        {
            var method = AccessTools.Method(type, target) ?? throw new MissingMethodException($"{type.Name}.{target}");

            harmony.Patch(method,
                prefix: prefix == null ? null : new HarmonyMethod(typeof(PeoplePatches), prefix),
                postfix: postfix == null ? null : new HarmonyMethod(typeof(PeoplePatches), postfix));
        }
        catch (Exception ex)
        {
            _log($"{HookFailure.Prefix} {type.Name}.{target}, it keeps the game's rules: {ex.Message}");
        }
    }

    private static void NeighbourCheck() => NeighbourGuard.CheckStarts();

    private static void NeighbourChecked(ref bool __result) => __result = NeighbourGuard.CheckResult(__result);

    private static bool NeighbourDies() => AllowsDeath(NeighbourGuard, "the neighbour", ref _hasLoggedNeighbour);

    private static void CompanionCheck() => CompanionGuard.CheckStarts();

    private static void CompanionChecked(ref bool __result) => __result = CompanionGuard.CheckResult(__result);

    private static bool CompanionDies() => AllowsDeath(CompanionGuard, "the companion", ref _hasLoggedCompanion);

    /// <summary>A kept-alive death can come back on every survival check, so its line is only built the first time.</summary>
    private static bool AllowsDeath(DeathGuard guard, string who, ref bool hasLogged)
    {
        var allows = guard.AllowsDeath();

        if (!allows && !hasLogged)
        {
            hasLogged = true;
            LogOnce("death " + who, $"{who} would have died; kept alive at the weakest stage (logged once per game start)");
        }

        return allows;
    }

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code.</summary>
    private static bool ScaleMishapChance(ref float mishapMul)
    {
        try
        {
            if (MishapOdds.SkipsRoll(_mishapChance)) return false;

            mishapMul = MishapOdds.Multiplier(mishapMul, _mishapChance);
        }
        catch (Exception ex)
        {
            LogOnce("mishap failure", $"scaling a stranger's mishap chance failed, it keeps the game's chance (logged once): {ex}");
        }

        return true;
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
