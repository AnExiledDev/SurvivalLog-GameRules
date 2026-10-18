using System;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>Whole Floor Climate's heat pool now lives in this mod; this carries a player's move from one to the other.</summary>
internal static class ClimateMove
{
    private const string OldConfigFile = "com.dev.survivallog.wholefloorclimate.cfg";
    private const string OldPluginFile = "WholeFloorClimate.dll";

    /// <summary>
    /// Copies the Pool switches from the old mod's config the first time this mod's config gets a Pool section, so a
    /// player who moves over keeps the zones they picked. Never throws: a failed copy only leaves the defaults.
    /// </summary>
    public static void ImportOldPool(ConfigFile config, bool hadPoolSection, ConfigEntry<bool>[] poolEntries, Action<string> log)
    {
        if (hadPoolSection) return;

        try
        {
            var oldPath = Path.Combine(Paths.ConfigPath, OldConfigFile);

            if (!File.Exists(oldPath)) return;

            var oldValues = OldPoolConfig.Read(File.ReadAllText(oldPath));

            foreach (var entry in poolEntries)
            {
                if (oldValues.TryGetValue(entry.Definition.Key, out var value)) entry.Value = value;
            }

            config.Save();
            log($"copied the Pool settings from Whole Floor Climate's config ({string.Join(", ", oldValues.Select(pair => $"{pair.Key}={pair.Value}"))})");
        }
        catch (Exception ex)
        {
            log($"could not copy the Pool settings from Whole Floor Climate's config, the defaults stand: {ex.Message}");
        }
    }

    /// <summary>True when this mod's config file already holds a Pool section, read before anything is bound.</summary>
    public static bool HasPoolSection(ConfigFile config)
    {
        try
        {
            return File.Exists(config.ConfigFilePath) && File.ReadAllText(config.ConfigFilePath).Contains("[Pool]");
        }
        catch (Exception)
        {
            return true;
        }
    }

    /// <summary>
    /// True while the old mod is still installed. Both would pool the same heat and every zone would count it twice,
    /// so the pool stays with the old mod until it is removed.
    /// </summary>
    public static bool IsOldModInstalled()
    {
        try
        {
            return Directory.EnumerateFiles(Paths.PluginPath, OldPluginFile, SearchOption.AllDirectories).Any();
        }
        catch (Exception)
        {
            return false;
        }
    }
}
