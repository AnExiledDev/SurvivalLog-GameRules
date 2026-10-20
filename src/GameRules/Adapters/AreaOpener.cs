using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Opens the run's locked areas once the run has loaded and the disaster has happened. The game keeps them as tags in
/// the world's TagManager, whose AddTags and RemoveTags tell the floor menu and the house's roofs and doors to update
/// straight away, and the tags are saved with the run. Checked every second, so a new run is opened as the disaster
/// hits, a loaded save straight away, and a run the game closes again is opened again. The tutorial is left alone.
/// </summary>
internal sealed class AreaOpener
{
    private const float CheckSeconds = 1f;

    private readonly Action<string> _log;
    private readonly HashSet<int> _reportedRefusals = new();
    private float _nextCheckAt;

    public AreaOpener(Action<string> log)
    {
        _log = log;
    }

    public void Tick(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        if (ReadyRun() is not var (tags, events, role, isBeforeDisaster)) return;

        OpenLockedAreas(tags, role, isBeforeDisaster);
        EndStaleEvents(tags, events);
    }

    private void OpenLockedAreas(TagManager tags, int role, bool isBeforeDisaster)
    {
        var change = OpenAreas.Plan(role, isBeforeDisaster, tags.HasTag);

        if (change.IsEmpty) return;

        if (change.Remove.Count > 0) tags.RemoveTags(ToGameList(change.Remove));

        if (change.Add.Count > 0) tags.AddTags(ToGameList(change.Add));

        _log($"opened the locked areas: added {string.Join(", ", change.Add)}; removed {(change.Remove.Count > 0 ? string.Join(", ", change.Remove) : "nothing")}");
    }

    /// <summary>
    /// Ends an opening task that was already on the task list when its area opened, such as Repair Stairs from a saved
    /// run. The game's own cancel removes it from the list; it refuses while events are locked and is tried again then.
    /// </summary>
    private void EndStaleEvents(TagManager tags, DynamicEventManager events)
    {
        foreach (var eventId in OpenAreas.StaleEvents(tags.HasTag))
        {
            if (!events.IsEventRunning(eventId)) continue;

            if (events.CancelTaskEvent(eventId)) _log($"ended task {eventId}, its area is already open");
            else if (_reportedRefusals.Add(eventId)) _log($"the game would not end task {eventId} yet; trying again each second");
        }
    }

    private static (TagManager Tags, DynamicEventManager Events, int Role, bool IsBeforeDisaster)? ReadyRun()
    {
        var main = GameSingleton.Existing<BattleMain>();
        var world = GameSingleton.Existing<BattleLogicWorld>();
        var save = GameSingleton.Existing<GameSaveManager>()?.gameSave;

        if (main == null || !main._IsLoadingFinished || main.IsGameOver) return null;

        if (world == null || !world.IsUpdate || world.GameModeKey != GameMode.Default || save == null) return null;

        var tags = world._TagManager;
        var clock = world._GameTimeManager;
        var events = world._DynamicEventManager;

        return tags == null || clock == null || events == null ? null : (tags, events, save.PlayerSelectId, clock.WorldStateType == WorldStateType.PreDisaster);
    }

    private static Il2CppSystem.Collections.Generic.List<string> ToGameList(System.Collections.Generic.IEnumerable<string> tags)
    {
        var list = new Il2CppSystem.Collections.Generic.List<string>();

        foreach (var tag in tags) list.Add(tag);

        return list;
    }
}
