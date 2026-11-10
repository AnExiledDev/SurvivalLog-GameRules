using System;
using ExiledToolkit.Game;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;

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

    private static readonly FirstTime LoggedEvents = new();
    private static PlagueBlock _block = PlagueBlock.Game;
    private static bool _isHooked;
    private static Action<string> _log = _ => { };
    private static PatchKit? _hooks;

    /// <summary>
    /// Called on the main thread at start and after each change. The hook goes in the first time a block needs it and
    /// stays in; with both switches off, the plague comes as in the game again.
    /// </summary>
    public static void Use(PatchKit hooks, PlagueBlock block, Action<string> log)
    {
        _block = block;
        _log = log;
        _hooks = hooks;

        if (!block.BlocksAny || _isHooked) return;

        _isHooked = true;

        hooks.Patch(typeof(PlaguePatches), typeof(DynamicEventManager), nameof(DynamicEventManager.CreateDynamicEventArgs),
            new[] { typeof(DynamicEventConfig), typeof(bool), typeof(bool), typeof(bool) }, prefix: nameof(SkipPlagueEvent),
            consequence: "the plague comes as in the game");
    }

    /// <summary>Runs inside the game's call; an escaping exception would reach native code, so a failure lets the event through.</summary>
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
            _hooks?.Fail("checking a plague event, it goes ahead as in the game", ex);

            return true;
        }
    }

    /// <summary>A save whose tags cannot be read counts as underway, so the plague's ending is never held back by mistake.</summary>
    private static bool IsPlagueUnderway()
    {
        var tags = GameSingleton.Existing<BattleLogicWorld>()?._TagManager;

        return tags == null || tags.HasTag(PlagueTag);
    }
}
