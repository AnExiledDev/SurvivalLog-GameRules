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
    private static Action<string> _log = _ => { };
    private static readonly HashSet<string> LoggedOnce = new();

    public static void Apply(Harmony harmony, RuleSettings settings, Action<string> log)
    {
        _log = log;
        _spoilageSpeed = settings.SpoilageSpeed;

        if (settings.StopMoldSpread)
        {
            Hook(harmony, typeof(FoodPollutionManager), "SpreadOneRound", Type.EmptyTypes,
                "mold no longer spreads to food nearby; mold already there still clears as in the game", prefix: nameof(SkipMoldSpread));
        }

        if (settings.ChangesSpoilage)
        {
            Hook(harmony, typeof(EnvAreaHelper), nameof(EnvAreaHelper.GetFoodDecayScale), new[] { typeof(int) },
                $"food in every room spoils at {settings.SpoilageSpeed:0.##}x the game's speed", postfix: nameof(ScaleFoodDecay));
        }
    }

    private static void Hook(Harmony harmony, Type type, string target, Type[] parameters, string what, string? prefix = null, string? postfix = null)
    {
        try
        {
            var method = AccessTools.Method(type, target, parameters) ?? throw new MissingMethodException($"{type.Name}.{target}");

            harmony.Patch(method,
                prefix: prefix == null ? null : new HarmonyMethod(typeof(RulePatches), prefix),
                postfix: postfix == null ? null : new HarmonyMethod(typeof(RulePatches), postfix));
            _log(what);
        }
        catch (Exception ex)
        {
            _log($"could not hook {type.Name}.{target}, it keeps the game's rules: {ex.Message}");
        }
    }

    /// <summary>The spread pass alone; the same loop still runs the shrink pass, so existing mold fades as usual.</summary>
    private static bool SkipMoldSpread() => false;

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
