using System;
using BepInEx.Configuration;
using GameRules.Core;
using HarmonyLib;

namespace GameRules.Settings;

internal static class FoodAndHealthRules
{
    /// <summary>Binds this group's settings and applies them now, and again after each change.</summary>
    public static void Start(ConfigFile config, Harmony harmony, TableChanges table, Action<string> log)
    {
    }
}
