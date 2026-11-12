using System;
using System.Collections.Generic;
using ExiledToolkit.Game;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// The Harmony hooks for mold and spoilage. Each target was checked in GameAssembly.dll: no relative jump or call in
/// its first 14 bytes. Static because Harmony patches are.
/// </summary>
internal static class RulePatches
{
    private const string KeepsGameRules = "it keeps the game's rules";

    private static float _spoilageSpeed = 1f;
    private static bool _isMoldStopped;
    private static bool _isMoldHooked;
    private static bool _isSpoilageHooked;
    private static bool _hasLoggedScale;
    private static Action<string> _log = _ => { };
    private static PatchKit? _hooks;
    private static readonly HashSet<string> LoggedOnce = new();

    /// <summary>
    /// Called on the main thread at start and after each change. Each hook goes in the first time its setting leaves the
    /// game's rule and stays in; back at the game's rule, it lets the game's answer through.
    /// </summary>
    public static void Use(PatchKit hooks, RuleSettings settings, Action<string> log)
    {
        _log = log;
        _hooks = hooks;
        _isMoldStopped = settings.StopMoldSpread;
        _spoilageSpeed = settings.SpoilageSpeed;

        if (settings.StopMoldSpread && !_isMoldHooked)
        {
            _isMoldHooked = true;

            hooks.Patch(typeof(RulePatches), typeof(FoodPollutionManager), "SpreadOneRound", Type.EmptyTypes, prefix: nameof(SkipMoldSpread),
                consequence: KeepsGameRules);
        }

        if (settings.ChangesSpoilage && !_isSpoilageHooked)
        {
            _isSpoilageHooked = true;

            hooks.Patch(typeof(RulePatches), typeof(EnvAreaHelper), nameof(EnvAreaHelper.GetFoodDecayScale), new[] { typeof(int) }, postfix: nameof(ScaleFoodDecay),
                consequence: KeepsGameRules);
        }

        log(settings.StopMoldSpread ? "mold no longer spreads to food nearby; mold already there still clears as usual" : "mold spreads like it normally does");
        log(settings.ChangesSpoilage ? $"food in every room spoils at {settings.SpoilageSpeed:0.##}x the game's speed" : "food spoils at the game's speed");
    }

    /// <summary>The spread pass alone; the same loop still runs the shrink pass, so existing mold fades as usual.</summary>
    private static bool SkipMoldSpread() => !_isMoldStopped;

    /// <summary>Runs inside the game's call; an escaping exception would reach native code.</summary>
    private static void ScaleFoodDecay(int areaId, ref float __result)
    {
        try
        {
            var gameScale = __result;

            __result = SpoilageScale.Apply(gameScale, _spoilageSpeed);

            // Runs for every food item, so the line is only built while it is still unlogged.
            if (__result != gameScale && !_hasLoggedScale)
            {
                _hasLoggedScale = true;
                LogOnce("spoilage", $"first scaled food decay: zone {areaId} {gameScale:0.###} in the game, {__result:0.###} with the mod");
            }
        }
        catch (Exception ex)
        {
            _hooks?.Fail("scaling food decay, food keeps the game's speed", ex);
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
