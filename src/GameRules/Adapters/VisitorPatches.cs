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

    public static void Apply(Harmony harmony, VisitorBlock block, Action<string> log)
    {
        if (!block.BlocksAny) return;

        _block = block;
        _log = log;

        var who = block.NoDoorSmasher && block.NoDebtCollector ? "the door-smasher and the debt collector"
            : block.NoDoorSmasher ? "the door-smasher" : "the debt collector";

        Hook(harmony, typeof(DynamicEventManager), nameof(DynamicEventManager.CreateDynamicEventArgs),
            new[] { typeof(DynamicEventConfig), typeof(bool), typeof(bool), typeof(bool) }, nameof(SkipBlockedEvent), $"{who} no longer visit");

        if (block.NoDebtCollector)
        {
            Hook(harmony, typeof(PhoneSMSManager), nameof(PhoneSMSManager.DeliverSMS), new[] { typeof(int), typeof(int) },
                nameof(SkipBlockedText), "the debt collector's warning text is no longer sent");
        }
    }

    private static void Hook(Harmony harmony, Type type, string target, Type[] parameters, string prefix, string what)
    {
        try
        {
            var method = AccessTools.Method(type, target, parameters) ?? throw new MissingMethodException($"{type.Name}.{target}");

            harmony.Patch(method, prefix: new HarmonyMethod(typeof(VisitorPatches), prefix));
            _log(what);
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
