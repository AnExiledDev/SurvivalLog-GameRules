using System;
using System.Collections.Generic;
using ExiledToolkit.Game;
using GameCore.HotUpdate.Battle.Logic;

namespace GameRules.Adapters;

/// <summary>
/// Asks every table writer for one more pass when a save loads or a trip starts (AgentManager.DeduplicateAgentSavesForLoad
/// runs for both). The tables are built once per game start, so this only covers the case where something rebuilds
/// them. The target was checked in GameAssembly.dll (no relative jump or call in its first 14 bytes); Package Bank and
/// Wind Power hook it too. Static because a Harmony patch has nowhere else to look.
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
            "DeduplicateAgentSavesForLoad",
            postfix: nameof(AfterLoad),
            consequence: "a game table that is built again would keep the game's values until the next setting change");
    }

    /// <summary>Only sets flags; the writes happen at the next frame's check, outside the game's load.</summary>
    private static void AfterLoad()
    {
        for (var i = 0; i < Redos.Count; i++) Redos[i]();
    }
}
