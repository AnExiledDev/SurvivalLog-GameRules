using System;
using System.Threading;
using ExiledToolkit.Game;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameRules.Core;

namespace GameRules.Adapters;

/// <summary>
/// The hooks for skill XP and stamina. ProficiencyManager.AddExp (0x2824d00, the overload with the actualAdded out
/// value; the other overload calls it) gets the XP before the game takes off its crafting cut, so FullCraftingXp and
/// SkillXp work together. AttributeComponent.CostAttrValue (0x3082350) gets every stat change before it is applied.
/// Both targets were checked in GameAssembly.dll: no relative jump or call in their first 14 bytes. Static because
/// Harmony patches are.
/// </summary>
internal static class BodyPatches
{
    private static readonly XpScale Xp = new();
    private static float _actionCost = 1f;
    private static float _stimulantStrength = 1f;
    private static long _heroId;
    private static bool _isXpHooked;
    private static bool _isStaminaHooked;
    private static PatchKit? _hooks;

    /// <summary>The player's id, set by <see cref="StatCaps.Tick"/>; 0 before a game is loaded.</summary>
    public static long HeroId
    {
        get => Interlocked.Read(ref _heroId);
        set => Interlocked.Exchange(ref _heroId, value);
    }

    /// <summary>True while a stamina setting needs the player's id.</summary>
    public static bool NeedsHero => Volatile.Read(ref _actionCost) != 1f || Volatile.Read(ref _stimulantStrength) != 1f;

    /// <summary>
    /// Called on the main thread at start and after each change. Each hook goes in the first time its setting leaves the
    /// game's rule and stays in; back at the game's rule, it lets the game's numbers through.
    /// </summary>
    public static void Use(PatchKit hooks, BodySettings settings)
    {
        _hooks = hooks;

        lock (Xp) Xp.Use(settings.SkillXp);

        Volatile.Write(ref _actionCost, settings.ActionStaminaCost);
        Volatile.Write(ref _stimulantStrength, settings.StimulantStrength);

        if (settings.SkillXp != 1f && !_isXpHooked)
        {
            _isXpHooked = true;

            hooks.Patch(typeof(BodyPatches), typeof(ProficiencyManager), nameof(ProficiencyManager.AddExp),
                new[]
                {
                    typeof(ProficiencyType), typeof(string), typeof(int), typeof(int), typeof(int),
                    typeof(ProficiencyExpSource), typeof(int).MakeByRefType(),
                },
                prefix: nameof(ScaleXp), consequence: "skills gain XP at the game's rate");
        }

        if (NeedsHero && !_isStaminaHooked)
        {
            _isStaminaHooked = true;

            hooks.Patch(typeof(BodyPatches), typeof(AttributeComponent), nameof(AttributeComponent.CostAttrValue),
                new[]
                {
                    typeof(AttrName), typeof(float), typeof(long), typeof(EffectArgsType), typeof(bool), typeof(bool),
                    typeof(VitalityChangeSourceType), typeof(int),
                },
                prefix: nameof(ScaleStamina), consequence: "stamina costs and stimulants stay at the game's numbers");
        }
    }

    /// <summary>Runs inside the game's call; an escaping exception would reach native code.</summary>
    private static void ScaleXp(ProficiencyType type, ref int baseExp, ref int discoveryExp, ProficiencyExpSource source)
    {
        try
        {
            if (!XpScale.Scales((int)source)) return;

            lock (Xp)
            {
                baseExp = Xp.Base((int)type, baseExp);
                discoveryExp = Xp.Discovery(discoveryExp);
            }
        }
        catch (Exception ex)
        {
            _hooks?.Fail("scaling skill XP, it keeps the game's amount", ex);
        }
    }

    /// <summary>Runs on every stat change of every character, so anything that is not the player's stamina leaves at once.</summary>
    private static void ScaleStamina(AttributeComponent __instance, AttrName attrName, ref float value, EffectArgsType argsType,
        VitalityChangeSourceType sourceType, int configId)
    {
        if (attrName != AttrName.Stamina) return;

        var actionCost = Volatile.Read(ref _actionCost);
        var stimulantStrength = Volatile.Read(ref _stimulantStrength);

        if (actionCost == 1f && stimulantStrength == 1f) return;

        try
        {
            var hero = HeroId;

            if (hero == 0 || __instance.ownerInstanceId != hero) return;

            value *= StaminaChange.Factor(value, (int)argsType, (int)sourceType, configId, actionCost, stimulantStrength);
        }
        catch (Exception ex)
        {
            _hooks?.Fail("scaling a stamina change, it keeps the game's amount", ex);
        }
    }
}
