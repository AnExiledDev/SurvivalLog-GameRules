using System;
using System.Collections.Generic;
using System.Reflection;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;
using HarmonyLib;

namespace GameRules.Adapters;

/// <summary>
/// The hooks for device power use, the power cut and heavy-rain leaks, cold weather and cold resistance, the stray
/// zombie leader chance and stray zombie kill loot. Every dynamic event is built by
/// DynamicEventManager.CreateDynamicEventArgs, whose callers all handle it returning nothing, so a held-back event is
/// simply not built. Each target was checked in GameAssembly.dll: no relative jump or call in its first 14 bytes.
/// Static because Harmony patches are.
/// </summary>
internal static class HouseZombiePatches
{
    private const string LeaderChanceKey = "StrayZombieLeaderChanceCurve";
    private const string ColdResistEffect = "Buff/AE_PersonalColdResist";

    private static readonly HashSet<string> LoggedOnce = new();
    private static readonly Random LootRoll = new();
    private static HouseZombieSettings _settings = HouseZombieSettings.Game;
    private static HouseEventBlock _events = HouseEventBlock.Game;
    private static MethodInfo? _spawnKillLoot;
    private static bool _isSpawningExtraLoot;
    private static bool _isPowerHooked;
    private static bool _isEventHooked;
    private static bool _isColdHooked;
    private static bool _isResistHooked;
    private static bool _isLeaderHooked;
    private static bool _isLootHooked;
    private static Action<string> _log = _ => { };

    /// <summary>
    /// Called on the main thread at start and after each change. Each hook goes in the first time its setting leaves the
    /// game's rule and stays in; back at the game's rule, it lets the game's own answer through.
    /// </summary>
    public static void Use(Harmony harmony, HouseZombieSettings settings, Action<string> log)
    {
        _log = log;
        _settings = settings;
        _events = settings.Events;

        if (settings.DevicePowerUse != 1f && !_isPowerHooked)
        {
            _isPowerHooked = true;

            Hook(harmony, typeof(PowerManager), "GetEffectivePowerCost", new[] { typeof(Config_Furniture), typeof(long) }, postfix: nameof(ScalePowerCost));
        }

        if (_events.BlocksAny && !_isEventHooked)
        {
            _isEventHooked = true;

            Hook(harmony, typeof(DynamicEventManager), nameof(DynamicEventManager.CreateDynamicEventArgs),
                new[] { typeof(DynamicEventConfig), typeof(bool), typeof(bool), typeof(bool) }, prefix: nameof(SkipHouseEvent));
        }

        if (settings.ColdWaveStrength != 1f && !_isColdHooked)
        {
            _isColdHooked = true;

            Hook(harmony, typeof(WeatherManager), "GetColdValue", Type.EmptyTypes, postfix: nameof(ScaleCold));
        }

        if (settings.ColdResistBonus > 0 && !_isResistHooked)
        {
            _isResistHooked = true;

            Hook(harmony, typeof(BuffComponent), "GetTalentEffectRatio", new[] { typeof(string) }, postfix: nameof(AddColdResist));
        }

        if (settings.StrayZombieLeaderChance != 1f && !_isLeaderHooked)
        {
            _isLeaderHooked = true;

            Hook(harmony, typeof(StrayZombieSettings), "GetString", new[] { typeof(string) }, postfix: nameof(ScaleLeaderChance));
        }

        if (settings.RerollsStrayLoot && !_isLootHooked)
        {
            _isLootHooked = true;
            _spawnKillLoot = Hook(harmony, typeof(StrayZombieManager), "SpawnKillLoot", Type.EmptyTypes, prefix: nameof(RollStrayLoot));
        }
    }

    /// <summary>The hooked method, so the stray loot hook can call it again; null when the hook could not go in.</summary>
    private static MethodInfo? Hook(Harmony harmony, Type type, string target, Type[] parameters, string? prefix = null, string? postfix = null)
    {
        try
        {
            var method = AccessTools.Method(type, target, parameters) ?? throw new MissingMethodException($"{type.Name}.{target}");

            harmony.Patch(method,
                prefix: prefix == null ? null : new HarmonyMethod(typeof(HouseZombiePatches), prefix),
                postfix: postfix == null ? null : new HarmonyMethod(typeof(HouseZombiePatches), postfix));

            return method;
        }
        catch (Exception ex)
        {
            _log($"{HookFailure.Prefix} {type.Name}.{target}, it keeps the game's rules: {ex.Message}");

            return null;
        }
    }

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code.</summary>
    private static void ScalePowerCost(ref float __result)
    {
        try
        {
            __result *= _settings.DevicePowerUse;
        }
        catch (Exception ex)
        {
            LogOnce("power failure", $"scaling a device's power use failed, it uses the game's amount (logged once): {ex}");
        }
    }

    /// <summary>Runs inside the game's own call; a failure lets the event through.</summary>
    private static bool SkipHouseEvent(DynamicEventConfig config, ref DynamicEventArgs? __result)
    {
        try
        {
            if (config == null) return true;

            var eventId = config._EventId_k__BackingField;

            if (!_events.BlocksEvent(eventId, GameDay())) return true;

            __result = null;
            LogOnce($"event {eventId}", $"held back event {eventId} (logged once per event)");

            return false;
        }
        catch (Exception ex)
        {
            LogOnce("event failure", $"checking a house event failed, events go ahead as in the game (logged once): {ex}");

            return true;
        }
    }

    /// <summary>The game day counted from 0, as the event table counts it; null before a run has loaded.</summary>
    private static int? GameDay() => GameSingleton.Existing<BattleLogicWorld>()?._GameTimeManager?.GetDay();

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code.</summary>
    private static void ScaleCold(ref int __result)
    {
        try
        {
            __result = ColdWaveScale.Cold(__result, _settings.ColdWaveStrength);
        }
        catch (Exception ex)
        {
            LogOnce("cold failure", $"scaling the weather's cold failed, it is as cold as in the game (logged once): {ex}");
        }
    }

    /// <summary>Called for every talent effect the game looks up; only the player's personal cold resistance changes.</summary>
    private static void AddColdResist(string effectConfigName, ref float __result)
    {
        try
        {
            if (effectConfigName != ColdResistEffect) return;

            __result = ColdWaveScale.ResistTiers(__result, _settings.ColdResistBonus);
        }
        catch (Exception ex)
        {
            LogOnce("resist failure", $"adding cold resistance failed, it is the game's own (logged once): {ex}");
        }
    }

    /// <summary>Called for every stray zombie setting the game reads as text; only the leader chance curve changes.</summary>
    private static void ScaleLeaderChance(string key, ref string __result)
    {
        try
        {
            if (key != LeaderChanceKey) return;

            __result = StrayZombieOdds.LeaderCurve(__result, _settings.StrayZombieLeaderChance);
        }
        catch (Exception ex)
        {
            LogOnce("leader failure", $"scaling the stray zombie leader chance failed, it is the game's own (logged once): {ex}");
        }
    }

    /// <summary>
    /// Rolls a killed stray zombie's loot as many times as the setting asks: the game's own call is the last roll, and
    /// the extra rolls call it again, passing straight through this hook. A failure keeps the game's one roll.
    /// </summary>
    private static bool RollStrayLoot(StrayZombieManager __instance)
    {
        if (_isSpawningExtraLoot) return true;

        try
        {
            var rolls = _settings.RerollsStrayLoot ? StrayZombieOdds.LootRolls(_settings.StrayZombieLoot, LootRoll.NextDouble()) : 1;

            if (rolls == 0) return false;

            SpawnExtraLoot(__instance, rolls - 1);
        }
        catch (Exception ex)
        {
            LogOnce("loot failure", $"rolling extra stray zombie loot failed, it drops the game's loot (logged once): {ex}");
        }

        return true;
    }

    private static void SpawnExtraLoot(StrayZombieManager manager, int extraRolls)
    {
        if (extraRolls <= 0 || _spawnKillLoot == null) return;

        _isSpawningExtraLoot = true;

        try
        {
            for (var roll = 0; roll < extraRolls; roll++) _spawnKillLoot.Invoke(manager, null);
        }
        finally
        {
            _isSpawningExtraLoot = false;
        }
    }

    private static void LogOnce(string key, string message)
    {
        lock (LoggedOnce)
        {
            if (!LoggedOnce.Add(key)) return;
        }

        _log(message);
    }
}
