using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;
using HarmonyLib;

namespace GameRules.Adapters;

/// <summary>
/// The game's side of opened areas. The story events that open an area are built by
/// DynamicEventManager.CreateDynamicEventArgs, whose callers all handle it returning nothing, so an event for an area
/// that is already open is never built. Before the disaster the game greys out every building spot on a key-locked
/// floor through ConfigManager.IsMapSlotOnUnlockableFloor, whatever the tags say; a spot whose floor tag is there is
/// reported as open instead. Both targets were checked in GameAssembly.dll: no relative jump or call in their first 14
/// bytes. Static because Harmony patches are.
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
        Hook(harmony, typeof(ConfigManager), nameof(ConfigManager.IsMapSlotOnUnlockableFloor), new[] { typeof(string) },
            null, new HarmonyMethod(typeof(AreaPatches), nameof(OpenSpotOnOpenedFloor)),
            "opened floors can be built on before the disaster");
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

    /// <summary>Only ever turns a locked answer into an open one, and only for a floor whose tag is there; a failure keeps the game's answer.</summary>
    private static void OpenSpotOnOpenedFloor(ConfigManager __instance, string slotName, ref bool __result)
    {
        try
        {
            if (!__result || RunTags() is not { } tags) return;

            if (OpenAreas.IsFloorStillLocked(FloorTag(__instance, slotName), tags.HasTag)) return;

            __result = false;
        }
        catch (Exception ex)
        {
            LogOnce("spot failure", $"checking a building spot failed, it stays as the game has it (logged once): {ex}");
        }
    }

    /// <summary>The tag that opens the floor a building spot is on, or null when the spot or its floor is unknown.</summary>
    private static string? FloorTag(ConfigManager config, string slotName)
    {
        if (string.IsNullOrEmpty(slotName) || !config.TryGetMapSlotByName(slotName, out var slot) || slot == null) return null;

        var points = config._Config_MapPoint_Dict;

        return points != null && points.TryGetValue(slot._MapID_k__BackingField, out var point) && point != null
            ? point._UnlockTag_k__BackingField
            : null;
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
