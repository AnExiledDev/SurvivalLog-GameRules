using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;
using HarmonyLib;

namespace GameRules.Adapters;

/// <summary>
/// Skips the story events that would open an area that is already open. They are built by
/// DynamicEventManager.CreateDynamicEventArgs, whose callers all handle it returning nothing, so such an event is never
/// built. The target was checked in GameAssembly.dll: no relative jump or call in its first 14 bytes. Static because
/// Harmony patches are.
/// </summary>
internal static class AreaPatches
{
    private static Action<string> _log = _ => { };
    private static readonly HashSet<string> LoggedOnce = new();

    public static void Apply(Harmony harmony, Action<string> log)
    {
        _log = log;

        Hook(harmony, typeof(DynamicEventManager), nameof(DynamicEventManager.CreateDynamicEventArgs),
            new[] { typeof(DynamicEventConfig), typeof(bool), typeof(bool), typeof(bool) },
            new HarmonyMethod(typeof(AreaPatches), nameof(SkipOpeningEvent)), null,
            "the events that open an area are skipped once it is open");
    }

    private static void Hook(Harmony harmony, Type type, string target, Type[] parameters, HarmonyMethod? prefix, HarmonyMethod? postfix, string what)
    {
        try
        {
            var method = AccessTools.Method(type, target, parameters) ?? throw new MissingMethodException($"{type.Name}.{target}");

            harmony.Patch(method, prefix: prefix, postfix: postfix);
            _log(what);
        }
        catch (Exception ex)
        {
            _log($"could not hook {type.Name}.{target}, that part of opening areas is off: {ex.Message}");
        }
    }

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code, so a failure lets the event through.</summary>
    private static bool SkipOpeningEvent(DynamicEventConfig config, ref DynamicEventArgs? __result)
    {
        try
        {
            if (config == null || RunTags() is not { } tags) return true;

            var eventId = config._EventId_k__BackingField;

            if (!OpenAreas.SkipsEvent(eventId, tags.HasTag)) return true;

            __result = null;
            LogOnce($"event {eventId}", $"skipped event {eventId}, its area is already open (logged once per event)");

            return false;
        }
        catch (Exception ex)
        {
            LogOnce("event failure", $"checking an area event failed, events go ahead as in the game (logged once): {ex}");

            return true;
        }
    }

    private static TagManager? RunTags() => GameSingleton.Existing<BattleLogicWorld>()?._TagManager;

    private static void LogOnce(string key, string message)
    {
        lock (LoggedOnce)
        {
            if (!LoggedOnce.Add(key)) return;
        }

        _log(message);
    }
}
