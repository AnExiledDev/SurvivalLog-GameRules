using System;
using ExiledToolkit.Game;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;

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
    private static PatchKit? _hooks;
    private static readonly FirstTime LoggedEvents = new();

    public static void Apply(PatchKit hooks, Action<string> log)
    {
        _log = log;
        _hooks = hooks;

        hooks.Patch(typeof(AreaPatches), typeof(DynamicEventManager), nameof(DynamicEventManager.CreateDynamicEventArgs),
            new[] { typeof(DynamicEventConfig), typeof(bool), typeof(bool), typeof(bool) }, prefix: nameof(SkipOpeningEvent),
            done: "the events that open an area are skipped once it is open", consequence: "that part of opening areas is off");
    }

    /// <summary>Runs inside the game's call; an escaping exception would reach native code, so a failure lets the event through.</summary>
    private static bool SkipOpeningEvent(DynamicEventConfig config, ref DynamicEventArgs? __result)
    {
        try
        {
            if (config == null) return true;

            var eventId = config._EventId_k__BackingField;

            // Every event the game builds passes through here, so the save's tags are only read for an opening event.
            if (!OpenAreas.OpensArea(eventId) || RunTags() is not { } tags) return true;

            if (!OpenAreas.SkipsEvent(eventId, tags.HasTag)) return true;

            __result = null;
            if (LoggedEvents.Of(eventId)) _log($"skipped event {eventId}, its area is already open (logged once per event)");

            return false;
        }
        catch (Exception ex)
        {
            _hooks?.Fail("checking an area event, events go ahead as usual", ex);

            return true;
        }
    }

    private static TagManager? RunTags() => GameSingleton.Existing<BattleLogicWorld>()?._TagManager;
}
