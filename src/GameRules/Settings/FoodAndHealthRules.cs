using System;
using BepInEx.Configuration;
using ExiledToolkit;
using ExiledToolkit.Game;
using GameRules.Adapters;
using GameRules.Core;

namespace GameRules.Settings;

internal static class FoodAndHealthRules
{
    private const string Group = "food-health";

    /// <summary>Binds this group's settings and applies them now, and again after each change.</summary>
    public static void Start(ConfigFile config, PatchKit hooks, TableChanges table, Action<string> log)
    {
        var entries = new Entries(config);
        var sickness = new SicknessChanceTable(log);

        RulesTicker.Checks.Add(sickness.Tick);

        LiveSettings.Watch(() =>
        {
            var settings = entries.Read();

            table.Set(Group, settings.TableChanges());
            sickness.Use(settings.EffectChanges());
            FoodFatiguePatches.Use(hooks, settings.NoFoodFatigue, log);
            PlaguePatches.Use(hooks, settings.PlagueBlock, log);
            Report(settings, log);
        }, entries.All);
    }

    private static void Report(FoodHealthSettings settings, Action<string> log)
    {
        log(Times(settings.RawFoodSickChance, "eating raw food makes you sick at {0}x the game's chance", "eating raw food makes you sick at the game's chance"));
        log(settings.NoFoodFatigue ? "food fatigue (Appetite Loss) is off" : "food fatigue (Appetite Loss) works as usual");
        log(Times(settings.FoodFatigueLength, "food fatigue lasts {0}x the game's time", "food fatigue lasts the game's time"));
        log(Times(settings.FoodFatiguePenalty, "food fatigue cuts what a food gives by {0}x the game's cut", "food fatigue cuts what a food gives like it normally does"));
        log($"the Disinfectant Spray reaches {settings.MoldSprayRadius} cells around the spot you spray and uses {settings.MoldSprayCost} charge(s) per moldy item");
        log(Times(settings.MoldyFoodSpoilSpeed, "moldy food spoils at {0}x the game's speed", "moldy food spoils at the game's speed"));
        log(Times(settings.MoldSpreadInterval, "mold waits {0}x the game's time between spreads", "mold spreads at the game's pace"));
        log($"mold spreads to at most {settings.MoldSpreadItems} items each time and starts on {settings.MoldSeedsDay1} items, before the game's extra for severe mold");
        log(Times(settings.RatVisitChance, "rats visit at {0}x the game's chance", "rats visit at the game's chance"));
        log(Times(settings.RatEscapeTime, "a visiting rat stays {0}x the game's time before it escapes", "a visiting rat stays the game's time"));
        log(DescribePlague(settings.PlagueBlock));
        log(Times(settings.WindChillChance, "Wind Chill is caught at {0}x the game's chance", "Wind Chill is caught at the game's chance"));
        log(Times(settings.FeverChance, "a High Temperature is caught at {0}x the game's chance", "a High Temperature is caught at the game's chance"));
    }

    private static string Times(float factor, string changed, string game) =>
        factor == 1f ? game : string.Format(changed, factor.ToString("0.##"));

    private static string DescribePlague(PlagueBlock block)
    {
        if (block.NoPlague) return "the Outbreak crisis no longer comes; one already underway gets no worse and still ends like it normally would";

        if (block.NoWorsening) return "the Outbreak crisis still comes but never worsens";

        return "the Outbreak crisis comes and worsens like it normally does";
    }

    private sealed class Entries
    {
        private readonly ConfigEntry<float> _rawFoodSickChance;
        private readonly ConfigEntry<bool> _noFoodFatigue;
        private readonly ConfigEntry<float> _foodFatigueLength;
        private readonly ConfigEntry<float> _foodFatiguePenalty;
        private readonly ConfigEntry<int> _moldSprayRadius;
        private readonly ConfigEntry<int> _moldSprayCost;
        private readonly ConfigEntry<float> _moldyFoodSpoilSpeed;
        private readonly ConfigEntry<float> _moldSpreadInterval;
        private readonly ConfigEntry<int> _moldSpreadItems;
        private readonly ConfigEntry<int> _moldSeedsDay1;
        private readonly ConfigEntry<float> _ratVisitChance;
        private readonly ConfigEntry<float> _ratEscapeTime;
        private readonly ConfigEntry<bool> _noPlague;
        private readonly ConfigEntry<bool> _noPlagueWorsening;
        private readonly ConfigEntry<float> _windChillChance;
        private readonly ConfigEntry<float> _feverChance;

        public Entries(ConfigFile config)
        {
            var game = FoodHealthSettings.Game;

            _rawFoodSickChance = config.Bind("Food", "RawFoodSickChance", game.RawFoodSickChance,
                new ConfigDescription(
                    "How likely raw food is to make you sick with Diarrhea and the like, times the normal chance. 0 means raw food never makes you sick. 1 is the default.\n" +
                    "生吃食物后生病（「腹泻」等）的几率，为游戏原本的倍数。0 为永不生病。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 5f), SettingTags.WorldRule));
            _noFoodFatigue = config.Bind("Food", "NoFoodFatigue", game.NoFoodFatigue,
                new ConfigDescription("Eating the same food again and again never causes Appetite Loss, and Appetite Loss you already have clears within moments. Off leaves the game as it is.\n" +
                "反复吃同一种食物不会再导致「厌食」，已有的厌食也会很快消除。关闭后与游戏原本相同。", null, SettingTags.WorldRule));
            _foodFatigueLength = config.Bind("Food", "FoodFatigueLength", game.FoodFatigueLength,
                new ConfigDescription(
                    "How long Appetite Loss lasts and how much each repeat meal adds to it, times the normal time: 0.5 is half as long. 1 is the default.\n" +
                    "「厌食」持续的时间以及每次重复进食增加的时间，为游戏原本的倍数：0.5 为一半。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0.1f, 5f), SettingTags.WorldRule));
            _foodFatiguePenalty = config.Bind("Food", "FoodFatiguePenalty", game.FoodFatiguePenalty,
                new ConfigDescription(
                    "How much Appetite Loss cuts what food gives you, times the normal cut. Normally food gives half while you have Appetite Loss. 0 means no cut, and 2 means the food gives nothing. 1 is the default.\n" +
                    "「厌食」时食物效果减少的程度，为游戏原本的倍数（游戏原本减半）：0 为不减少，2 为食物没有任何效果。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 2f), SettingTags.WorldRule));
            _moldSprayRadius = config.Bind("Mold", "MoldSprayRadius", game.MoldSprayRadius,
                new ConfigDescription(
                    "How many cells around the sprayed cell the Disinfectant Spray also cleans. 0 cleans only the cell you spray. 1 is the default.\n" +
                    "「消毒喷雾」除了喷洒的格子外，还能清理周围多少格。0 为只清理喷洒的格子。1 为游戏原本的数值。",
                    new AcceptableValueRange<int>(0, 5), SettingTags.WorldRule));
            _moldSprayCost = config.Bind("Mold", "MoldSprayCost", game.MoldSprayCost,
                new ConfigDescription(
                    "How many uses of the Disinfectant Spray it takes to clean one moldy item. 1 is the default and the lowest you can set.\n" +
                    "清理一件发霉物品需要消耗「消毒喷雾」的次数。1 为游戏原本的数值，也是最低值。",
                    new AcceptableValueRange<int>(FoodHealthSettings.GameSprayCost, 5), SettingTags.WorldRule));
            _moldyFoodSpoilSpeed = config.Bind("Mold", "MoldyFoodSpoilSpeed", game.MoldyFoodSpoilSpeed,
                new ConfigDescription(
                    "How fast moldy food spoils, times the normal speed. Moldy food normally spoils much faster than fresh food, and 0.2 brings it close to the speed of fresh food. 1 is the default.\n" +
                    "发霉食物的腐坏速度，为游戏原本的倍数。发霉食物本来就比新鲜食物腐坏得快得多，0.2 约等于新鲜食物的速度。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0.2f, 2f), SettingTags.WorldRule));
            _moldSpreadInterval = config.Bind("Mold", "MoldSpreadInterval", game.MoldSpreadInterval,
                new ConfigDescription(
                    "The time between mold spreads, times the normal time: 2 spreads half as often, 0.5 twice as often. 1 is the default.\n" +
                    "「霉菌」两次扩散之间的时间，为游戏原本的倍数：2 为扩散频率减半，0.5 为两倍频率。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0.5f, 4f), SettingTags.WorldRule));
            _moldSpreadItems = config.Bind("Mold", "MoldSpreadItems", game.MoldSpreadItems,
                new ConfigDescription(
                    "The most items mold spreads to each time it spreads. The game adds more when mold is severe. 2 is the default.\n" +
                    "「霉菌」每次扩散最多感染的物品数量。霉菌严重时游戏会再增加。2 为游戏原本的数值。",
                    new AcceptableValueRange<int>(1, 6), SettingTags.WorldRule));
            _moldSeedsDay1 = config.Bind("Mold", "MoldSeedsDay1", game.MoldSeedsDay1,
                new ConfigDescription(
                    "How many items turn moldy when mold first appears. The game adds more when mold is severe. 3 is the default.\n" +
                    "「霉菌」刚出现时发霉的物品数量。霉菌严重时游戏会再增加。3 为游戏原本的数值。",
                    new AcceptableValueRange<int>(1, 10), SettingTags.WorldRule));
            _ratVisitChance = config.Bind("Rats", "RatVisitChance", game.RatVisitChance,
                new ConfigDescription(
                    "How likely a rat is to visit each day, times the normal chance, including the extra chance from stored food and its cap. 0 means rats never visit. 1 is the default. During a Rat Infestation the game still sends rats to every food store.\n" +
                    "每天老鼠来访的几率，为游戏原本的倍数，储存食物带来的额外几率及其上限也一同调整。0 为老鼠永不来访。1 为游戏原本的数值。「鼠患」期间游戏仍会让老鼠光顾每一处存放食物的地方。",
                    new AcceptableValueRange<float>(0f, 3f), SettingTags.WorldRule));
            _ratEscapeTime = config.Bind("Rats", "RatEscapeTime", game.RatEscapeTime,
                new ConfigDescription(
                    "How long a visiting rat stays before it escapes, times the normal time: 2 gives you twice as long to catch it. 1 is the default.\n" +
                    "来访的老鼠逃走前停留的时间，为游戏原本的倍数：2 为有两倍的时间抓住它。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0.5f, 5f), SettingTags.WorldRule));
            _noPlague = config.Bind("Sickness", "NoPlague", game.NoPlague,
                new ConfigDescription("The Outbreak crisis never comes: no Feeling Unwell, no outbreak and no worsening. If one has already started in your save, it gets no worse and ends like it normally would. Off leaves the game as it is.\n" +
                "「疫病」危机不会到来：不会出现「身体不舒服」、疫病发作和「疫病加重」。如果存档中疫病已经开始，它不会再加重，并照常在「疫病消退」时结束。关闭后与游戏原本相同。", null, SettingTags.WorldRule));
            _noPlagueWorsening = config.Bind("Sickness", "NoPlagueWorsening", game.NoPlagueWorsening,
                new ConfigDescription("The Outbreak crisis still comes but never reaches Disease Worsens, even if rats are left alone. Off leaves the game as it is.\n" +
                "「疫病」危机仍会到来，但即使放任老鼠不管，也不会出现「疫病加重」。关闭后与游戏原本相同。", null, SettingTags.WorldRule));
            _windChillChance = config.Bind("Sickness", "WindChillChance", game.WindChillChance,
                new ConfigDescription(
                    "How likely you are to catch Wind Chill in a cold wave without heating, times the normal chance. 0 means never. 1 is the default. Wind Chill you already have runs its course.\n" +
                    "寒潮中没有取暖时患上「风寒」的几率，为游戏原本的倍数。0 为永不患上。1 为游戏原本的数值。已经患上的风寒照常持续到结束。",
                    new AcceptableValueRange<float>(0f, 2f), SettingTags.WorldRule));
            _feverChance = config.Bind("Sickness", "FeverChance", game.FeverChance,
                new ConfigDescription(
                    "How likely you are to get a High Temperature in a cold wave without heating, times the normal chance. 0 means never. 1 is the default. A High Temperature you already have runs its course.\n" +
                    "寒潮中没有取暖时「发烧」的几率，为游戏原本的倍数。0 为永不发烧。1 为游戏原本的数值。已经发烧的照常持续到结束。",
                    new AcceptableValueRange<float>(0f, 2f), SettingTags.WorldRule));

            All = new ConfigEntryBase[]
            {
                _rawFoodSickChance, _noFoodFatigue, _foodFatigueLength, _foodFatiguePenalty,
                _moldSprayRadius, _moldSprayCost, _moldyFoodSpoilSpeed, _moldSpreadInterval, _moldSpreadItems, _moldSeedsDay1,
                _ratVisitChance, _ratEscapeTime, _noPlague, _noPlagueWorsening, _windChillChance, _feverChance,
            };
        }

        public ConfigEntryBase[] All { get; }

        public FoodHealthSettings Read() => new(
            _rawFoodSickChance.Value,
            _noFoodFatigue.Value,
            _foodFatigueLength.Value,
            _foodFatiguePenalty.Value,
            _moldSprayRadius.Value,
            _moldSprayCost.Value,
            _moldyFoodSpoilSpeed.Value,
            _moldSpreadInterval.Value,
            _moldSpreadItems.Value,
            _moldSeedsDay1.Value,
            _ratVisitChance.Value,
            _ratEscapeTime.Value,
            _noPlague.Value,
            _noPlagueWorsening.Value,
            _windChillChance.Value,
            _feverChance.Value);
    }
}
