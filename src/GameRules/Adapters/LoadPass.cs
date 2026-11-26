using System;
using System.Collections.Generic;
using ExiledToolkit.Game;
using GameCore.HotUpdate.Battle.Logic;

namespace GameRules.Adapters;

/// <summary>
/// Asks every table writer for one more pass at each start of play: a load, a new game, a trip out and the way home all
/// run AgentManager.StartBattle. The tables are built once per game start, so this only covers the case where something
/// rebuilds them. The target was checked in GameAssembly.dll (no relative jump or call in its first 14 bytes). Static
/// because a Harmony patch has nowhere else to look.
/// </summary>
internal static class LoadPass
{
    internal static readonly List<Action> Redos = new();

    public static void Apply(PatchKit hooks)
    {
        if (Redos.Count == 0) return;

        hooks.Patch(
            typeof(LoadPass),
            typeof(AgentManager),
            "StartBattle",
            postfix: nameof(AfterStart),
            consequence: "a game table that is built again would keep the game's values until the next setting change");
    }

    /// <summary>Only sets flags; the writes happen at the next frame's check, outside the game's load.</summary>
    private static void AfterStart()
    {
        for (var i = 0; i < Redos.Count; i++) Redos[i]();
    }
}
