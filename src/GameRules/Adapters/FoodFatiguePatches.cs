using System;
using System.Collections.Generic;
using ExiledToolkit.Game;
using GameCore.HotUpdate.Battle.Logic;

namespace GameRules.Adapters;

/// <summary>
/// Turns food fatigue (Appetite Loss) off. PoorAppetiteComponent.AddHistory is the only place a meal is counted toward
/// it, so skipping it means no food ever wears out. Fatigue the character already has, from before the switch or from
/// the save, is cleared after CleanExpiredRecords, the game's regular pass over it; CheckAndUpdateBuff then takes
/// the Appetite Loss buff and its display away as the game does when fatigue runs out. Both targets were checked in
/// GameAssembly.dll: no relative jump or call in their first 14 bytes. Static because Harmony patches are.
/// </summary>
internal static class FoodFatiguePatches
{
    private static readonly HashSet<string> LoggedOnce = new();
    private static bool _isFatigueOff;
    private static bool _isHooked;
    private static Action<string> _log = _ => { };
    private static PatchKit? _hooks;

    /// <summary>
    /// Called on the main thread at start and after each change. The hooks go in the first time fatigue is turned off and
    /// stay in; turned back on, meals count toward it again as usual.
    /// </summary>
    public static void Use(PatchKit hooks, bool noFoodFatigue, Action<string> log)
    {
        _log = log;
        _hooks = hooks;
        _isFatigueOff = noFoodFatigue;

        if (!noFoodFatigue || _isHooked) return;

        _isHooked = true;

        Hook(hooks, "AddHistory", new[] { typeof(int) }, prefix: nameof(SkipMealCount));
        Hook(hooks, "CleanExpiredRecords", Type.EmptyTypes, postfix: nameof(ClearFatigue));
    }

    private static void Hook(PatchKit hooks, string target, Type[] parameters, string? prefix = null, string? postfix = null) =>
        hooks.Patch(typeof(FoodFatiguePatches), typeof(PoorAppetiteComponent), target, parameters, prefix, postfix,
            consequence: "food fatigue works as usual");

    private static bool SkipMealCount() => !_isFatigueOff;

    /// <summary>Runs inside the game's call; an escaping exception would reach native code.</summary>
    private static void ClearFatigue(PoorAppetiteComponent __instance)
    {
        try
        {
            if (!_isFatigueOff || __instance == null) return;

            var tiredOf = __instance._PoorAppetiteList_k__BackingField;

            if (tiredOf == null || tiredOf.Count == 0) return;

            tiredOf.Clear();
            __instance._History_k__BackingField?.Clear();
            __instance.CheckAndUpdateBuff();
            LogOnce("cleared", "cleared the Appetite Loss the character already had (logged once)");
        }
        catch (Exception ex)
        {
            _hooks?.Fail("clearing Appetite Loss, it wears off as usual", ex);
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
