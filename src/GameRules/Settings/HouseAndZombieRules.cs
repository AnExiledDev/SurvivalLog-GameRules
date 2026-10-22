using System;
using BepInEx.Configuration;
using GameRules.Adapters;
using GameRules.Core;
using HarmonyLib;

namespace GameRules.Settings;

internal static class HouseAndZombieRules
{
    private const string Group = "house-zombies";

    /// <summary>Binds this group's settings and applies them now, and again after each change.</summary>
    public static void Start(ConfigFile config, Harmony harmony, TableChanges table, Action<string> log)
    {
        var entries = new Entries(config);

        OptionalLive.Watch(() =>
        {
            var settings = entries.Read();

            table.Set(Group, settings.TableChanges());
            HouseZombiePatches.Use(harmony, settings, log);
            Report(settings, log);
        }, entries.All);
    }

    private static void Report(HouseZombieSettings settings, Action<string> log)
    {
        log($"devices use {settings.DevicePowerUse:0.##}x the game's power");
        log(PowerCutLine(settings));
        log(settings.NoStormLeaks ? "heavy rain never leaks into the house" : "heavy rain can leak into the house as in the game");
        log($"cold weather is {settings.ColdWaveStrength:0.##}x as cold as in the game");
        log($"personal cold resistance gets {settings.ColdResistBonus} extra level(s)");
        log(StrayZombieLine(settings));
        log($"stray zombie leaders come at {settings.StrayZombieLeaderChance:0.##}x the game's chance");
        log($"a killed stray zombie drops {settings.StrayZombieLoot:0.##}x the game's loot");
        log(settings.NoAdaptiveWaves ? "zombie hordes always come at full strength" : "zombie hordes adapt to your defenses as in the game");
        log($"zombie hordes leave {settings.WaveLoot:0.##}x the game's loot for the zombies killed");
    }

    private static string PowerCutLine(HouseZombieSettings settings)
    {
        if (settings.NoPowerCut) return "the city power is never cut";
        if (settings.PowerCutDelay > 0) return $"the city power cut comes {settings.PowerCutDelay} day(s) later than in the game";

        return "the city power is cut as in the game";
    }

    private static string StrayZombieLine(HouseZombieSettings settings)
    {
        if (settings.NoStrayZombies) return "stray zombies never come";

        return $"stray zombies come from day {settings.StrayZombieStartDay}, {settings.StrayZombieCooldownHours} hour(s) apart, " +
            $"up to {settings.StrayZombieMaxPerDay} visit(s) a day and {settings.StrayZombieMaxAlive} at once";
    }

    private sealed class Entries
    {
        private readonly ConfigEntry<float> _devicePowerUse;
        private readonly ConfigEntry<int> _powerCutDelay;
        private readonly ConfigEntry<bool> _noPowerCut;
        private readonly ConfigEntry<bool> _noStormLeaks;
        private readonly ConfigEntry<float> _coldWaveStrength;
        private readonly ConfigEntry<int> _coldResistBonus;
        private readonly ConfigEntry<bool> _noStrayZombies;
        private readonly ConfigEntry<int> _strayZombieStartDay;
        private readonly ConfigEntry<int> _strayZombieCooldownHours;
        private readonly ConfigEntry<int> _strayZombieMaxPerDay;
        private readonly ConfigEntry<int> _strayZombieMaxAlive;
        private readonly ConfigEntry<float> _strayZombieLeaderChance;
        private readonly ConfigEntry<float> _strayZombieLoot;
        private readonly ConfigEntry<bool> _noAdaptiveWaves;
        private readonly ConfigEntry<float> _waveLoot;

        public Entries(ConfigFile config)
        {
            var game = HouseZombieSettings.Game;

            _devicePowerUse = config.Bind("Power", "DevicePowerUse", game.DevicePowerUse,
                new ConfigDescription(
                    "How much power switched-on devices use, times the game's use: 0.5 is half. Cooking and shredding jobs, the compressor, defense devices firing and the signal station use the game's amount. 1 is the game's own.\n" +
                    "开启的电器消耗的电量，为游戏原本的倍数：0.5 为一半。烹饪和粉碎工作、压缩机、防御设施开火和信号站的耗电与游戏原本相同。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 3f)));
            _powerCutDelay = config.Bind("Power", "PowerCutDelay", game.PowerCutDelay,
                new ConfigDescription(
                    "How many days later than in the game the city power is cut. 0 is the game's own. A cut that already happened stays.\n" +
                    "市电被切断（剧情事件「停电开始」）比游戏原本推迟的天数。0 为游戏原本的数值。已经停电的不会恢复。",
                    new AcceptableValueRange<int>(0, 100)));
            _noPowerCut = config.Bind("Power", "NoPowerCut", game.NoPowerCut,
                "The city power is never cut, so the house keeps grid power all game. The story events that follow the cut never come, and the diesel generator cannot be started. A cut that already happened stays. Off: as in the game.\n" +
                "市电永远不会被切断（剧情事件「停电开始」不会出现），房子整局都有市电。停电之后的剧情事件不会出现，柴油发电机也无法启动。已经停电的不会恢复。关闭后与游戏原本相同。");
            _noStormLeaks = config.Bind("House", "NoStormLeaks", game.NoStormLeaks,
                "Heavy rain never leaks in through the windows (the Heavy Rain event never comes), so there are no leaks to deal with. Off: as in the game.\n" +
                "暴雨不会再从窗缝渗入（「暴雨侵袭」事件不会出现），不需要处理漏雨。关闭后与游戏原本相同。");
            _coldWaveStrength = config.Bind("Cold", "ColdWaveStrength", game.ColdWaveStrength,
                new ConfigDescription(
                    "How cold cold waves, frosty clear days and cold rain are, times the game's cold, rounded to the game's whole steps: 0 makes them no colder than a mild day, 2 is twice as cold. It changes how cold you feel, how plants grow and whether supported strangers count the day as cold. The weather and the forecast stay as in the game. 1 is the game's own.\n" +
                    "「寒潮」、霜晴、寒晴、凛晴和寒雨的寒冷程度，为游戏原本的倍数，取整到游戏的等级：0 为与温和的日子一样，2 为两倍寒冷。影响你感受到的寒冷、植物生长，以及你支援的陌生人是否算作寒冷天。天气和天气预报与游戏原本相同。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 3f)));
            _coldResistBonus = config.Bind("Cold", "ColdResistBonus", game.ColdResistBonus,
                new ConfigDescription(
                    "Extra levels of Personal Cold Resistance for your character, added to any from talents: each level makes the cold feel one step milder. 0 is the game's own.\n" +
                    "角色额外获得的「个人抗寒」等级，与天赋给予的相加：每一级让寒冷的感受减轻一档。0 为游戏原本的数值。",
                    new AcceptableValueRange<int>(0, 3)));
            _noStrayZombies = config.Bind("Zombies", "NoStrayZombies", game.NoStrayZombies,
                "Stray zombies never wander up to the house between zombie hordes. Zombie hordes still come. Off: as in the game.\n" +
                "零散丧尸不会在「尸潮」之间游荡到房子附近。尸潮照常来袭。关闭后与游戏原本相同。");
            _strayZombieStartDay = config.Bind("Zombies", "StrayZombieStartDay", game.StrayZombieStartDay,
                new ConfigDescription(
                    "The day stray zombies start wandering up to the house. 7 is the game's own.\n" +
                    "零散丧尸开始游荡到房子附近的天数。7 为游戏原本的数值。",
                    new AcceptableValueRange<int>(1, 100)));
            _strayZombieCooldownHours = config.Bind("Zombies", "StrayZombieCooldownHours", game.StrayZombieCooldownHours,
                new ConfigDescription(
                    "In-game hours after a stray zombie visit before the next one can come. 4 is the game's own.\n" +
                    "一次零散丧尸来访后，到下一次可能来访之间的游戏内小时数。4 为游戏原本的数值。",
                    new AcceptableValueRange<int>(0, 48)));
            _strayZombieMaxPerDay = config.Bind("Zombies", "StrayZombieMaxPerDay", game.StrayZombieMaxPerDay,
                new ConfigDescription(
                    "The most stray zombie visits in one day. 3 is the game's own.\n" +
                    "一天内零散丧尸来访的最多次数。3 为游戏原本的数值。",
                    new AcceptableValueRange<int>(1, 10)));
            _strayZombieMaxAlive = config.Bind("Zombies", "StrayZombieMaxAlive", game.StrayZombieMaxAlive,
                new ConfigDescription(
                    "The most stray zombies around the house at once; no more come until some are killed. 3 is the game's own.\n" +
                    "房子附近同时存在的零散丧尸最多数量；在击杀一些之前不会再来。3 为游戏原本的数值。",
                    new AcceptableValueRange<int>(1, 20)));
            _strayZombieLeaderChance = config.Bind("Zombies", "StrayZombieLeaderChance", game.StrayZombieLeaderChance,
                new ConfigDescription(
                    "How likely a stray zombie visit is to be led by a crawler, times the game's chance (30% from about day 15 on). 0 means never. 1 is the game's own.\n" +
                    "零散丧尸来访时由「爬行者丧尸」带领的几率，为游戏原本的倍数（大约第 15 天起为 30%）。0 为永不出现。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 4f)));
            _strayZombieLoot = config.Bind("Zombies", "StrayZombieLoot", game.StrayZombieLoot,
                new ConfigDescription(
                    "How much a killed stray zombie drops, times the game's one item: 2 drops two, 1.5 drops one or two. 0 drops nothing. 1 is the game's own.\n" +
                    "击杀零散丧尸掉落的物品，为游戏原本一件的倍数：2 为两件，1.5 为一件或两件。0 为不掉落。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 5f)));
            _noAdaptiveWaves = config.Bind("Waves", "NoAdaptiveWaves", game.NoAdaptiveWaves,
                "Story zombie hordes always come at full strength. The game otherwise sends a weaker horde when your defenses are not ready for it. Off: as in the game.\n" +
                "剧情「尸潮」总是以全部强度来袭。游戏原本会在你的防御还没准备好时派来较弱的尸潮。关闭后与游戏原本相同。");
            _waveLoot = config.Bind("Waves", "WaveLoot", game.WaveLoot,
                new ConfigDescription(
                    "How much loot a zombie horde leaves for the zombies you kill, times the game's: 2 gives more kinds of item and more of each for the same kills, up to twice the game's limits. 1 is the game's own.\n" +
                    "「尸潮」按击杀数量留下的「战利品」，为游戏原本的倍数：2 为同样的击杀获得更多种类和数量的物品，上限为游戏原本的两倍。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0.5f, 5f)));

            All = new ConfigEntryBase[]
            {
                _devicePowerUse, _powerCutDelay, _noPowerCut, _noStormLeaks, _coldWaveStrength, _coldResistBonus,
                _noStrayZombies, _strayZombieStartDay, _strayZombieCooldownHours, _strayZombieMaxPerDay, _strayZombieMaxAlive,
                _strayZombieLeaderChance, _strayZombieLoot, _noAdaptiveWaves, _waveLoot,
            };
        }

        public ConfigEntryBase[] All { get; }

        public HouseZombieSettings Read() => new(
            _devicePowerUse.Value,
            _powerCutDelay.Value,
            _noPowerCut.Value,
            _noStormLeaks.Value,
            _coldWaveStrength.Value,
            _coldResistBonus.Value,
            _noStrayZombies.Value,
            _strayZombieStartDay.Value,
            _strayZombieCooldownHours.Value,
            _strayZombieMaxPerDay.Value,
            _strayZombieMaxAlive.Value,
            _strayZombieLeaderChance.Value,
            _strayZombieLoot.Value,
            _noAdaptiveWaves.Value,
            _waveLoot.Value);
    }
}
