using System;
using BepInEx.Configuration;
using ExiledToolkit;
using ExiledToolkit.Game;
using GameRules.Adapters;
using GameRules.Core;

namespace GameRules.Settings;

internal static class BodyRules
{
    /// <summary>Binds this group's settings and applies them now, and again after each change.</summary>
    public static void Start(ConfigFile config, PatchKit hooks, Action<string> log)
    {
        var entries = new Entries(config);
        var limits = new StimulantLimitTable(log);

        RulesTicker.Checks.Add(limits.Tick);
        RulesTicker.Checks.Add(StatCaps.Tick);

        LiveSettings.Watch(() =>
        {
            var settings = entries.Read();

            limits.Use(settings);
            BodyPatches.Use(hooks, settings);
            StatCaps.Use(hooks, settings);
            Report(settings, log);
        }, entries.All);
    }

    private static void Report(BodySettings settings, Action<string> log)
    {
        log(Times(settings.SkillXp, "skills gain {0}x the game's XP", "skills gain the game's XP"));
        log(Times(settings.ActionStaminaCost, "actions cost {0}x the game's stamina", "actions cost the game's stamina"));
        log(Times(settings.StimulantStrength, "stimulants give {0}x the game's stamina", "stimulants give the game's stamina"));
        log($"maximum satiety {Points(settings.ExtraMaxSatiety)}, maximum morale {Points(settings.ExtraMaxMorale)}");
    }

    private static string Times(float factor, string changed, string game) =>
        factor == 1f ? game : string.Format(changed, factor.ToString("0.##"));

    private static string Points(int extra) => extra == 0 ? "is the game's" : $"{extra:+0;-0} points";

    private sealed class Entries
    {
        private readonly ConfigEntry<float> _skillXp;
        private readonly ConfigEntry<float> _actionStaminaCost;
        private readonly ConfigEntry<float> _stimulantStrength;
        private readonly ConfigEntry<int> _stimulantDailyLimit;
        private readonly ConfigEntry<int> _extraMaxSatiety;
        private readonly ConfigEntry<int> _extraMaxMorale;

        public Entries(ConfigFile config)
        {
            var game = BodySettings.Game;

            _skillXp = config.Bind("Skills", "SkillXp", game.SkillXp,
                new ConfigDescription(
                    "How much proficiency every skill gains, times the game's amount: 2 levels skills twice as fast, 0.5 half as fast. 0 gives none. Works together with FullCraftingXp. 1 is the game's.\n" +
                    "所有技能获得的「熟练度」，为游戏原本的倍数：2 为升级快一倍，0.5 为慢一倍。0 为不获得。可与 FullCraftingXp 同时使用。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 10f), SettingTags.WorldRule));
            _actionStaminaCost = config.Bind("Stamina", "ActionStaminaCost", game.ActionStaminaCost,
                new ConfigDescription(
                    "How much stamina your actions cost (crafting, building, searching, working at furniture and the like), times the game's cost: 0.5 is half. 0 means actions cost no stamina. Stamina you lose over time or to a buff is not changed. 1 is the game's.\n" +
                    "行动消耗的「精力」（制作、建造、搜索、使用家具等），为游戏原本的倍数：0.5 为一半。0 为行动不消耗精力。随时间或因状态失去的精力不受影响。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 3f), SettingTags.WorldRule));
            _stimulantStrength = config.Bind("Stimulants", "StimulantStrength", game.StimulantStrength,
                new ConfigDescription(
                    "How much stamina coffee, Sports Drink, Adrenaline and Stimulants give, times the game's amount, including what they keep giving over the next hours: 2 is twice as much. 0 gives none. 1 is the game's.\n" +
                    "「咖啡」、「能量饮料」、「肾上腺激素」和「兴奋剂」恢复的「精力」，为游戏原本的倍数，之后几小时内持续恢复的部分也一同调整：2 为两倍。0 为不恢复。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 5f), SettingTags.WorldRule));
            _stimulantDailyLimit = config.Bind("Stimulants", "StimulantDailyLimit", game.StimulantDailyLimit,
                new ConfigDescription(
                    "How many times a day you can use each kind of coffee, Sports Drink, Adrenaline and Stimulants. Each kind counts on its own, so Coffee (Good) and Coffee (Perfect) each have their own count. The count starts again each day. 0 means no limit.\n" +
                    "每天可以使用每种「咖啡」、「能量饮料」、「肾上腺激素」和「兴奋剂」的次数。每种分开计算，例如不同品质的咖啡各有自己的次数。每天重新计数。0 为不限次数，与游戏原本相同。",
                    new AcceptableValueRange<int>(0, 10), SettingTags.WorldRule));
            _extraMaxSatiety = config.Bind("Stats", "ExtraMaxSatiety", game.ExtraMaxSatiety,
                new ConfigDescription(
                    "Points added to your maximum satiety: 50 lets you eat 50 more before you are full. A negative number lowers it. 0 is the game's.\n" +
                    "「饱腹（上限）」增加的点数：50 为吃饱前可以多吃 50 点。负数为降低上限。0 为游戏原本的数值。",
                    new AcceptableValueRange<int>(-50, 400), SettingTags.WorldRule));
            _extraMaxMorale = config.Bind("Stats", "ExtraMaxMorale", game.ExtraMaxMorale,
                new ConfigDescription(
                    "Points added to your maximum morale. A negative number lowers it. 0 is the game's.\n" +
                    "「心态（上限）」增加的点数。负数为降低上限。0 为游戏原本的数值。",
                    new AcceptableValueRange<int>(-50, 400), SettingTags.WorldRule));

            All = new ConfigEntryBase[]
            {
                _skillXp, _actionStaminaCost, _stimulantStrength, _stimulantDailyLimit, _extraMaxSatiety, _extraMaxMorale,
            };
        }

        public ConfigEntryBase[] All { get; }

        public BodySettings Read() => new(
            _skillXp.Value,
            _actionStaminaCost.Value,
            _stimulantStrength.Value,
            _stimulantDailyLimit.Value,
            _extraMaxSatiety.Value,
            _extraMaxMorale.Value);
    }
}
