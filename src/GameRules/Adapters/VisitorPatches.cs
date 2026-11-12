using System;
using ExiledToolkit.Game;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;

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
    private const string VisitorsStillCome = "those visitors still come";

    private static Action<string> _log = _ => { };
    private static PatchKit? _hooks;
    private static readonly FirstTime LoggedEvents = new();
    private static readonly FirstTime LoggedTexts = new();
    private static bool _isEventHooked;
    private static bool _isTextHooked;

    /// <summary>
    /// Called on the main thread at start and after each change. Each hook goes in the first time a block needs it and
    /// stays in; with a block turned off, those visitors and texts come back as usual.
    /// </summary>
    public static void Use(PatchKit hooks, VisitorBlock block, Action<string> log)
    {
        _block = block;
        _log = log;
        _hooks = hooks;

        if (block.BlocksAny && !_isEventHooked)
        {
            _isEventHooked = true;

            hooks.Patch(typeof(VisitorPatches), typeof(DynamicEventManager), nameof(DynamicEventManager.CreateDynamicEventArgs),
                new[] { typeof(DynamicEventConfig), typeof(bool), typeof(bool), typeof(bool) }, prefix: nameof(SkipBlockedEvent), consequence: VisitorsStillCome);
        }

        if (block.NoDebtCollector && !_isTextHooked)
        {
            _isTextHooked = true;

            hooks.Patch(typeof(VisitorPatches), typeof(PhoneSMSManager), nameof(PhoneSMSManager.DeliverSMS), new[] { typeof(int), typeof(int) },
                prefix: nameof(SkipBlockedText), consequence: VisitorsStillCome);
        }

        log(Describe(block));
    }

    private static string Describe(VisitorBlock block)
    {
        if (block.NoDoorSmasher && block.NoDebtCollector) return "the door-smasher and the debt collector no longer visit, and the debt collector's warning text is no longer sent";

        if (block.NoDoorSmasher) return "the door-smasher no longer visits; the debt collector still comes";

        if (block.NoDebtCollector) return "the debt collector no longer visits and its warning text is no longer sent; the door-smasher still comes";

        return "the door-smasher and the debt collector come like they normally do";
    }

    /// <summary>Runs inside the game's call; an escaping exception would reach native code, so a failure lets the event through.</summary>
    private static bool SkipBlockedEvent(DynamicEventConfig config, ref DynamicEventArgs? __result)
    {
        try
        {
            if (config == null) return true;

            var eventId = config._EventId_k__BackingField;

            if (!_block.BlocksEvent(eventId)) return true;

            __result = null;
            if (LoggedEvents.Of(eventId)) _log($"turned away event {eventId} (logged once per event)");

            return false;
        }
        catch (Exception ex)
        {
            _hooks?.Fail("checking an event, events go ahead as usual", ex);

            return true;
        }
    }

    private static bool SkipBlockedText(int smsId)
    {
        try
        {
            if (!_block.BlocksText(smsId)) return true;

            if (LoggedTexts.Of(smsId)) _log($"held back the debt collector's warning text {smsId}");

            return false;
        }
        catch (Exception ex)
        {
            _hooks?.Fail("checking a phone text, texts arrive as usual", ex);

            return true;
        }
    }
}
