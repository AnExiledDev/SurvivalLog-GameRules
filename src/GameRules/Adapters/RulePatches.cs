using System;
using System.Collections.Generic;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;
using HarmonyLib;

namespace GameRules.Adapters;

/// <summary>
/// The Harmony hooks for mold and spoilage. Each target was checked in GameAssembly.dll: no relative jump or call in
/// its first 14 bytes. Static because Harmony patches are.
/// </summary>
internal static class RulePatches
{
    private static float _spoilageSpeed = 1f;
    private static bool _isMoldStopped;
    private static bool _isMoldHooked;
    private static bool _isSpoilageHooked;
    private static Action<string> _log = _ => { };
    private static readonly HashSet<string> LoggedOnce = new();

    /// <summary>
    /// Called on the main thread at start and after each change. Each hook goes in the first time its setting leaves the
    /// game's rule and stays in; back at the game's rule, it lets the game's own answer through.
    /// </summary>
    public static void Use(Harmony harmony, RuleSettings settings, Action<string> log)
    {
        _log = log;
        _isMoldStopped = settings.StopMoldSpread;
        _spoilageSpeed = settings.SpoilageSpeed;

        if (settings.StopMoldSpread && !_isMoldHooked)
        {
            _isMoldHooked = true;

            Hook(harmony, typeof(FoodPollutionManager), "SpreadOneRound", Type.EmptyTypes, prefix: nameof(SkipMoldSpread));
        }

        if (settings.ChangesSpoilage && !_isSpoilageHooked)
        {
            _isSpoilageHooked = true;

            Hook(harmony, typeof(EnvAreaHelper), nameof(EnvAreaHelper.GetFoodDecayScale), new[] { typeof(int) }, postfix: nameof(ScaleFoodDecay));
        }

        log(settings.StopMoldSpread ? "mold no longer spreads to food nearby; mold already there still clears as in the game" : "mold spreads as in the game");
        log(settings.ChangesSpoilage ? $"food in every room spoils at {settings.SpoilageSpeed:0.##}x the game's speed" : "food spoils at the game's speed");
    }

    private static void Hook(Harmony harmony, Type type, string target, Type[] parameters, string? prefix = null, string? postfix = null)
    {
        try
        {
            var method = AccessTools.Method(type, target, parameters) ?? throw new MissingMethodException($"{type.Name}.{target}");

            harmony.Patch(method,
                prefix: prefix == null ? null : new HarmonyMethod(typeof(RulePatches), prefix),
                postfix: postfix == null ? null : new HarmonyMethod(typeof(RulePatches), postfix));
        }
        catch (Exception ex)
        {
            _log($"could not hook {type.Name}.{target}, it keeps the game's rules: {ex.Message}");
        }
    }

    /// <summary>The spread pass alone; the same loop still runs the shrink pass, so existing mold fades as usual.</summary>
    private static bool SkipMoldSpread() => !_isMoldStopped;

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code.</summary>
    private static void ScaleFoodDecay(int areaId, ref float __result)
    {
        try
        {
            var gameScale = __result;

            __result = SpoilageScale.Apply(gameScale, _spoilageSpeed);

            if (__result != gameScale) LogOnce("spoilage", $"first scaled food decay: zone {areaId} {gameScale:0.###} in the game, {__result:0.###} with the mod");
        }
        catch (Exception ex)
        {
            LogOnce("spoilage failure", $"scaling food decay failed, food keeps the game's speed (logged once): {ex}");
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
