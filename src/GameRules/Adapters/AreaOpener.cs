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
    private readonly TagMemo _tagMemo = new();
    private float _nextCheckAt;
    private IntPtr _doorsGoneIn;

    public AreaOpener(Action<string> log)
    {
        _log = log;
    }

    public void Tick(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        if (ReadyRun() is not var (world, tags, events, role, isBeforeDisaster)) return;

        _tagMemo.Use(tags.HasTag);

        try
        {
            OpenLockedAreas(tags, role, isBeforeDisaster);
            EndStaleEvents(events);
            RemoveOpenedDoors(world, role, isBeforeDisaster);
        }
        finally
        {
            _tagMemo.Use(null);
        }
    }

    private void OpenLockedAreas(TagManager tags, int role, bool isBeforeDisaster)
    {
        var change = OpenAreas.Plan(role, isBeforeDisaster, _tagMemo.HasTag);

        if (change.IsEmpty) return;

        // The stale-task check after this reads the tags as they now are.
        _tagMemo.Forget();

        if (change.Remove.Count > 0) tags.RemoveTags(ToGameList(change.Remove));

        if (change.Add.Count > 0) tags.AddTags(ToGameList(change.Add));

        _log($"opened the locked areas: added {string.Join(", ", change.Add)}; removed {(change.Remove.Count > 0 ? string.Join(", ", change.Remove) : "nothing")}");
    }

    /// <summary>
    /// Ends an opening task that was already on the task list when its area opened, such as Repair Stairs from a saved
    /// run. The game's own cancel removes it from the list; it refuses while events are locked and is tried again then.
    /// </summary>
    private void EndStaleEvents(DynamicEventManager events)
    {
        var stale = OpenAreas.StaleEvents(_tagMemo.HasTag);

        for (var i = 0; i < stale.Count; i++)
        {
            var eventId = stale[i];

            if (!events.IsEventRunning(eventId)) continue;

            if (events.CancelTaskEvent(eventId)) _log($"ended task {eventId}, its area is already open");
            else if (_reportedRefusals.Add(eventId)) _log($"the game would not end task {eventId} yet; trying again each second");
        }
    }

    /// <summary>
    /// Removes the door of each Warehouse Manager area that is open, the way the game's own unlock does, so a run opened
    /// by this mod (or a save from a version that only added the tags) can walk in. Once every door's area is open and the
    /// doors are gone, the run is not looked at again until another run loads.
    /// </summary>
    private void RemoveOpenedDoors(BattleLogicWorld world, int role, bool isBeforeDisaster)
    {
        if (world.Pointer == _doorsGoneIn) return;

        var doors = OpenAreas.DoorsToRemove(role, isBeforeDisaster, _tagMemo.HasTag);

        if (doors.Count == 0) return;

        var agents = world._AgentManager;

        if (agents == null) return;

        for (var i = 0; i < doors.Count; i++)
        {
            var found = agents.GetFurnituresByConfigId(doors[i]);

            if (found == null) continue;

            for (var j = found.Count - 1; j >= 0; j--)
            {
                AE_DisableFurniture.DemolishFurniture(found[j]);
                _log($"removed door {doors[i]}, its area is open");
            }
        }

        if (doors.Count == OpenAreas.WarehouseDoorCount) _doorsGoneIn = world.Pointer;
    }

    private static (BattleLogicWorld World, TagManager Tags, DynamicEventManager Events, int Role, bool IsBeforeDisaster)? ReadyRun()
    {
        var main = GameSingleton.Existing<BattleMain>();
        var world = GameSingleton.Existing<BattleLogicWorld>();
        var save = GameSingleton.Existing<GameSaveManager>()?.gameSave;

        if (main == null || !main._IsLoadingFinished || main.IsGameOver) return null;

        if (world == null || !world.IsUpdate || world.GameModeKey != GameMode.Default || save == null) return null;

        var tags = world._TagManager;
        var clock = world._GameTimeManager;
        var events = world._DynamicEventManager;

        return tags == null || clock == null || events == null ? null : (world, tags, events, save.PlayerSelectId, clock.WorldStateType == WorldStateType.PreDisaster);
    }

    private static Il2CppSystem.Collections.Generic.List<string> ToGameList(System.Collections.Generic.IEnumerable<string> tags)
    {
        var list = new Il2CppSystem.Collections.Generic.List<string>();

        foreach (var tag in tags) list.Add(tag);

        return list;
    }
}
