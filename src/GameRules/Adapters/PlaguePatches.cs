using System;
using System.Collections.Generic;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;
using HarmonyLib;

namespace GameRules.Adapters;

/// <summary>
/// Stops the Outbreak crisis or its worsening. Every plague event is built by DynamicEventManager.CreateDynamicEventArgs,
/// and every caller already handles it returning nothing, so a stopped event is simply never built. The crisis marks
/// the save with the tag CRISIS_PLAGUE_ACTIVE when it starts and clears it when it ends, which tells whether a plague
/// is underway. The target was checked in GameAssembly.dll: no relative jump or call in its first 14 bytes. Static
/// because Harmony patches are.
/// </summary>
internal static class PlaguePatches
{
    private const string PlagueTag = "CRISIS_PLAGUE_ACTIVE";

    private static readonly HashSet<string> LoggedOnce = new();
    private static readonly FirstTime LoggedEvents = new();
    private static PlagueBlock _block = PlagueBlock.Game;
    private static bool _isHooked;
    private static Action<string> _log = _ => { };

    /// <summary>
    /// Called on the main thread at start and after each change. The hook goes in the first time a block needs it and
    /// stays in; with both switches off, the plague comes as in the game again.
    /// </summary>
    public static void Use(Harmony harmony, PlagueBlock block, Action<string> log)
    {
        _block = block;
        _log = log;

        if (!block.BlocksAny || _isHooked) return;

        _isHooked = true;

        Hook(harmony);
    }

    private static void Hook(Harmony harmony)
    {
        try
        {
            var method = AccessTools.Method(typeof(DynamicEventManager), nameof(DynamicEventManager.CreateDynamicEventArgs),
                    new[] { typeof(DynamicEventConfig), typeof(bool), typeof(bool), typeof(bool) })
                ?? throw new MissingMethodException($"{nameof(DynamicEventManager)}.{nameof(DynamicEventManager.CreateDynamicEventArgs)}");

            harmony.Patch(method, prefix: new HarmonyMethod(typeof(PlaguePatches), nameof(SkipPlagueEvent)));
        }
        catch (Exception ex)
        {
            _log($"{HookFailure.Prefix} {nameof(DynamicEventManager)}.{nameof(DynamicEventManager.CreateDynamicEventArgs)}, the plague comes as in the game: {ex.Message}");
        }
    }

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code, so a failure lets the event through.</summary>
    private static bool SkipPlagueEvent(DynamicEventConfig config, ref DynamicEventArgs? __result)
    {
        try
        {
            if (config == null) return true;

            var eventId = config._EventId_k__BackingField;

            if (!_block.BlocksEvent(eventId, IsPlagueUnderway)) return true;

            __result = null;
            if (LoggedEvents.Of(eventId)) _log($"stopped plague event {eventId} (logged once per event)");

            return false;
        }
        catch (Exception ex)
        {
            LogOnce("event failure", $"checking a plague event failed, it goes ahead as in the game (logged once): {ex}");

            return true;
        }
    }

    /// <summary>A save whose tags cannot be read counts as underway, so the plague's ending is never held back by mistake.</summary>
    private static bool IsPlagueUnderway()
    {
        var tags = GameSingleton.Existing<BattleLogicWorld>()?._TagManager;

        return tags == null || tags.HasTag(PlagueTag);
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
