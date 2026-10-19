using System;
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
    private float _nextCheckAt;

    public AreaOpener(Action<string> log)
    {
        _log = log;
    }

    public void Tick(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        if (ReadyRun() is not var (tags, role, isBeforeDisaster)) return;

        var change = OpenAreas.Plan(role, isBeforeDisaster, tags.HasTag);

        if (change.IsEmpty) return;

        if (change.Remove.Count > 0) tags.RemoveTags(ToGameList(change.Remove));

        if (change.Add.Count > 0) tags.AddTags(ToGameList(change.Add));

        _log($"opened the locked areas: added {string.Join(", ", change.Add)}; removed {(change.Remove.Count > 0 ? string.Join(", ", change.Remove) : "nothing")}");
    }

    private static (TagManager Tags, int Role, bool IsBeforeDisaster)? ReadyRun()
    {
        var main = GameSingleton.Existing<BattleMain>();
        var world = GameSingleton.Existing<BattleLogicWorld>();
        var save = GameSingleton.Existing<GameSaveManager>()?.gameSave;

        if (main == null || !main._IsLoadingFinished || main.IsGameOver) return null;

        if (world == null || !world.IsUpdate || world.GameModeKey != GameMode.Default || save == null) return null;

        var tags = world._TagManager;
        var clock = world._GameTimeManager;

        return tags == null || clock == null ? null : (tags, save.PlayerSelectId, clock.WorldStateType == WorldStateType.PreDisaster);
    }

    private static Il2CppSystem.Collections.Generic.List<string> ToGameList(System.Collections.Generic.IEnumerable<string> tags)
    {
        var list = new Il2CppSystem.Collections.Generic.List<string>();

        foreach (var tag in tags) list.Add(tag);

        return list;
    }
}
