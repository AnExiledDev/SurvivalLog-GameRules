using System;
using System.Collections.Generic;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;
using HarmonyLib;

namespace GameRules.Adapters;

/// <summary>
/// Turns the door-smasher and the debt collector away. Every dynamic event, forced or rolled, is built by
/// DynamicEventManager.CreateDynamicEventArgs, and every caller already handles it returning nothing (it does so
/// itself while the event system is locked), so a blocked event is simply never built. The loan shark's warning text
/// goes through PhoneSMSManager.DeliverSMS. Both targets were checked in GameAssembly.dll: no relative jump or call
/// in their first 14 bytes. Static because Harmony patches are.
/// </summary>
internal static class VisitorPatches
{
    private static VisitorBlock _block = VisitorBlock.Game;
    private static Action<string> _log = _ => { };
    private static readonly HashSet<string> LoggedOnce = new();
    private static bool _isEventHooked;
    private static bool _isTextHooked;

    /// <summary>
    /// Called on the main thread at start and after each change. Each hook goes in the first time a block needs it and
    /// stays in; with a block turned off, those visitors and texts come as in the game again.
    /// </summary>
    public static void Use(Harmony harmony, VisitorBlock block, Action<string> log)
    {
        _block = block;
        _log = log;

        if (block.BlocksAny && !_isEventHooked)
        {
            _isEventHooked = true;

            Hook(harmony, typeof(DynamicEventManager), nameof(DynamicEventManager.CreateDynamicEventArgs),
                new[] { typeof(DynamicEventConfig), typeof(bool), typeof(bool), typeof(bool) }, nameof(SkipBlockedEvent));
        }

        if (block.NoDebtCollector && !_isTextHooked)
        {
            _isTextHooked = true;

            Hook(harmony, typeof(PhoneSMSManager), nameof(PhoneSMSManager.DeliverSMS), new[] { typeof(int), typeof(int) }, nameof(SkipBlockedText));
        }

        log(Describe(block));
    }

    private static string Describe(VisitorBlock block)
    {
        if (block.NoDoorSmasher && block.NoDebtCollector) return "the door-smasher and the debt collector no longer visit, and the debt collector's warning text is no longer sent";

        if (block.NoDoorSmasher) return "the door-smasher no longer visits; the debt collector comes as in the game";

        if (block.NoDebtCollector) return "the debt collector no longer visits and its warning text is no longer sent; the door-smasher comes as in the game";

        return "the door-smasher and the debt collector come as in the game";
    }

    private static void Hook(Harmony harmony, Type type, string target, Type[] parameters, string prefix)
    {
        try
        {
            var method = AccessTools.Method(type, target, parameters) ?? throw new MissingMethodException($"{type.Name}.{target}");

            harmony.Patch(method, prefix: new HarmonyMethod(typeof(VisitorPatches), prefix));
        }
        catch (Exception ex)
        {
            _log($"could not hook {type.Name}.{target}, those visitors still come: {ex.Message}");
        }
    }

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code, so a failure lets the event through.</summary>
    private static bool SkipBlockedEvent(DynamicEventConfig config, ref DynamicEventArgs? __result)
    {
        try
        {
            if (config == null) return true;

            var eventId = config._EventId_k__BackingField;

            if (!_block.BlocksEvent(eventId)) return true;

            __result = null;
            LogOnce($"event {eventId}", $"turned away event {eventId} (logged once per event)");

            return false;
        }
        catch (Exception ex)
        {
            LogOnce("event failure", $"checking an event failed, events go ahead as in the game (logged once): {ex}");

            return true;
        }
    }

    private static bool SkipBlockedText(int smsId)
    {
        try
        {
            if (!_block.BlocksText(smsId)) return true;

            LogOnce($"text {smsId}", $"held back the debt collector's warning text {smsId}");

            return false;
        }
        catch (Exception ex)
        {
            LogOnce("text failure", $"checking a phone text failed, texts arrive as in the game (logged once): {ex}");

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
