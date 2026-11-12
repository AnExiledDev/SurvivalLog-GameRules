using System;
using ExiledToolkit.Game;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// Adds the extra points to the player's MaxSatiety and MaxMorale. They go on the bonus part of each maximum through
/// AttributeComponent.AddStrengtheningValue, the way buffs add theirs, so bonuses from buffs and furniture still come and
/// go on top. The bonus part is not saved and is cleared when the game reuses the object for a new character (OnCreate,
/// 0x3083c90), so the mod adds it again. A load (LoadGame, 0x3083760) puts back a satiety or morale above the game's
/// maximum without checking it, and the next stat change would cut it down to the maximum, so the points go back on
/// right after the load. Both hook targets were checked in GameAssembly.dll: no relative jump or call in their first
/// 14 bytes. Also keeps <see cref="BodyPatches.HeroId"/> current. Static because Harmony patches are.
/// </summary>
internal static class StatCaps
{
    private const float CheckSeconds = 1f;

    private static readonly object Gate = new();
    private static readonly CapKeeper Satiety = new();
    private static readonly CapKeeper Morale = new();
    private static int _extraSatiety;
    private static int _extraMorale;
    private static float _nextCheckAt;
    private static bool _isHooked;
    private static PatchKit? _hooks;

    /// <summary>Called on the main thread at start and after each change; the next check adds or takes away the difference.</summary>
    public static void Use(PatchKit hooks, BodySettings settings)
    {
        _hooks = hooks;

        lock (Gate)
        {
            _extraSatiety = settings.ExtraMaxSatiety;
            _extraMorale = settings.ExtraMaxMorale;
        }

        _nextCheckAt = 0f;

        if ((settings.ExtraMaxSatiety == 0 && settings.ExtraMaxMorale == 0) || _isHooked) return;

        _isHooked = true;

        hooks.Patch(typeof(StatCaps), typeof(AttributeComponent), "OnCreate", postfix: nameof(Created),
            consequence: "a new game started in the same session may lose the extra maximum");
        hooks.Patch(typeof(StatCaps), typeof(AttributeComponent), "LoadGame", postfix: nameof(Loaded),
            consequence: "a satiety or morale saved above the game's maximum may drop to it after a load");
    }

    public static void Tick(float now)
    {
        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        var role = GameSingleton.Existing<BattleLogicWorld>()?._AgentManager?.GetLeadingRole();

        BodyPatches.HeroId = role?.InstanceId ?? 0;

        if (role == null || IsAtGameRule()) return;

        var attributes = AgentTools.GetAgentComponent<AttributeComponent>(role);

        if (attributes?.AttrDict == null) return;

        Apply(attributes);
    }

    /// <summary>With no extra wanted and none added, there is nothing to add or take back, so the player is not read.</summary>
    private static bool IsAtGameRule()
    {
        lock (Gate) return _extraSatiety == 0 && _extraMorale == 0 && Satiety.Added == 0 && Morale.Added == 0;
    }

    private static void Apply(AttributeComponent attributes)
    {
        var character = attributes.Pointer.ToInt64();

        lock (Gate)
        {
            Add(attributes, AttrName.MaxSatiety, Satiety.Next(character, _extraSatiety));
            Add(attributes, AttrName.MaxMorale, Morale.Next(character, _extraMorale));
        }
    }

    /// <summary>The whole-number overload: the maximums are whole numbers. isClearHistory true shows no floating text.</summary>
    private static void Add(AttributeComponent attributes, AttrName maximum, int points)
    {
        if (points != 0) attributes.AddStrengtheningValue(maximum, points, true);
    }

    /// <summary>Runs inside the game's call for every character made; an escaping exception would reach native code.</summary>
    private static void Created(AttributeComponent __instance)
    {
        try
        {
            var character = __instance.Pointer.ToInt64();

            lock (Gate)
            {
                Satiety.Forget(character);
                Morale.Forget(character);
            }
        }
        catch (Exception ex)
        {
            _hooks?.Fail("noting a new character, the extra maximum may be lost until the next game start", ex);
        }
    }

    /// <summary>The game's load runs for every character; only the player's gets the points.</summary>
    private static void Loaded(AttributeComponent __instance)
    {
        try
        {
            var agents = GameSingleton.Existing<BattleLogicWorld>()?._AgentManager;

            if (agents == null || __instance.ownerInstanceId != agents.LeadingRoleId || __instance.AttrDict == null) return;

            Apply(__instance);
        }
        catch (Exception ex)
        {
            _hooks?.Fail("adding the extra maximum after a load, the next check adds it a moment later", ex);
        }
    }
}
