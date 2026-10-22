using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using GameRules.Adapters;
using GameRules.Core;
using HarmonyLib;

namespace GameRules.Settings;

internal static class PeopleAndMoraleRules
{
    private const string Group = "people-morale";

    /// <summary>Binds this group's settings and applies them now, and again after each change.</summary>
    public static void Start(ConfigFile config, Harmony harmony, TableChanges table, Action<string> log)
    {
        var entries = new Entries(config);

        OptionalLive.Watch(() =>
        {
            var settings = entries.Read();

            table.Set(Group, settings.TableChanges());
            PeoplePatches.Use(harmony, settings, log);
            Report(settings, log);
        }, entries.All);
    }

    private static void Report(PeopleSettings settings, Action<string> log)
    {
        log(settings.NeverDie ? "the neighbour and companion never die" : "the neighbour and companion can die as in the game");

        if (settings.MishapChance != 1f) log($"supported strangers' mishaps happen at {settings.MishapChance:0.##}x the game's chance");
        if (settings.MishapsOff.Count > 0) log("supported strangers never suffer: " + string.Join(", ", settings.MishapsOff));
    }

    private sealed class Entries
    {
        private readonly ConfigEntry<bool> _neverDie;
        private readonly ConfigEntry<int> _rescueDeadline;
        private readonly ConfigEntry<float> _trustLoss;
        private readonly ConfigEntry<float> _mishapChance;
        private readonly Dictionary<Mishap, ConfigEntry<bool>> _mishapsOff;
        private readonly ConfigEntry<float> _commissionChance;
        private readonly ConfigEntry<float> _aidPlatformChance;
        private readonly ConfigEntry<float> _thoughtFrequency;
        private readonly ConfigEntry<float> _moraleGains;
        private readonly ConfigEntry<float> _vaseMoraleCap;
        private readonly ConfigEntry<bool> _fullCraftingXp;
        private readonly ConfigEntry<float> _pickupReach;

        public Entries(ConfigFile config)
        {
            var game = PeopleSettings.Game;

            _neverDie = config.Bind("Companions", "NeverDie", game.NeverDie,
                "The neighbour and your companion never die. When the game would let one die, she stays at her weakest instead and can still be rescued.\n" +
                "邻居和同行者不会死亡。游戏原本会让她死去时，她会停留在最虚弱的状态，仍然可以被救助。");
            _rescueDeadline = config.Bind("Companions", "RescueDeadline", game.RescueDeadline,
                new ConfigDescription(
                    "The day after the disaster on which the neighbour and your companion die if you have not started helping them. 21 is the game's own.\n" +
                    "灾难后的第几天，如果还没开始帮助邻居和同行者，她们就会死去。21 为游戏原本的数值。",
                    new AcceptableValueRange<int>(PeopleSettings.GameRescueDeadline, 120)));
            _trustLoss = config.Bind("Strangers", "TrustLoss", game.TrustLoss,
                new ConfigDescription(
                    "How much trust strangers you support lose each day when they go hungry or their letters go unanswered, times the game's loss. 0 means trust never drops. 1 is the game's own.\n" +
                    "你支援的陌生人在挨饿或来信没有回复时每天失去的「信赖」，为游戏原本的倍数。0 为信赖永不下降。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 3f)));
            _mishapChance = config.Bind("Strangers", "MishapChance", game.MishapChance,
                new ConfigDescription(
                    "How likely strangers you support are to suffer a mishap each day, times the game's chance. 0 means never. 1 is the game's own.\n" +
                    "你支援的陌生人每天「遭灾」的几率，为游戏原本的倍数。0 为永不遭灾。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 3f)));
            _mishapsOff = new Dictionary<Mishap, ConfigEntry<bool>>
            {
                [Mishap.Rat] = BindMishapOff(config, Mishap.Rat, "rats get into their food", "老鼠偷吃食物"),
                [Mishap.Fire] = BindMishapOff(config, Mishap.Fire, "a fire burns their materials", "「失火」烧掉材料"),
                [Mishap.Theft] = BindMishapOff(config, Mishap.Theft, "someone steals their materials", "材料被人偷走"),
                [Mishap.Facility] = BindMishapOff(config, Mishap.Facility, "something in their shelter breaks", "住处的设施损坏"),
                [Mishap.Pest] = BindMishapOff(config, Mishap.Pest, "pests eat their seeds", "虫害吃掉种子"),
                [Mishap.Injury] = BindMishapOff(config, Mishap.Injury, "they get hurt and use up medicine", "受伤并用掉药品"),
            };
            _commissionChance = config.Bind("Commissions", "CommissionChance", game.CommissionChance,
                new ConfigDescription(
                    "How likely a new commission is to arrive each day, times the game's chance: 0.5 is half as often. 1 is the game's own; 1.4 is close to every day.\n" +
                    "每天收到新委托的几率，为游戏原本的倍数：0.5 为一半。1 为游戏原本的数值；1.4 接近每天都有。",
                    new AcceptableValueRange<float>(0.1f, 1.4f)));
            _aidPlatformChance = config.Bind("Commissions", "AidPlatformChance", game.AidPlatformChance,
                new ConfigDescription(
                    "How likely a new request is to appear on the Aid Platform each day, times the game's chance: 0.5 is half as often. 1 is the game's own; 1.4 is close to every day.\n" +
                    "「救助平台」每天出现新求助的几率，为游戏原本的倍数：0.5 为一半。1 为游戏原本的数值；1.4 接近每天都有。",
                    new AcceptableValueRange<float>(0.1f, 1.4f)));
            _thoughtFrequency = config.Bind("Thoughts", "ThoughtFrequency", game.ThoughtFrequency,
                new ConfigDescription(
                    "How often your character thinks out loud and the guide chats, times the game's rate: 0.5 is half as often, 2 is twice as often. 1 is the game's own.\n" +
                    "角色自言自语和引导随机闲聊的频率，为游戏原本的倍数：0.5 为一半，2 为两倍。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0.1f, 4f)));
            _moraleGains = config.Bind("Morale", "MoraleGains", game.MoraleGains,
                new ConfigDescription(
                    "Morale from finishing a song on the music player and from a perfect harvest, times the game's amount. 0 gives none. 1 is the game's own.\n" +
                    "听完一首歌和完美收获带来的「心态」，为游戏原本的倍数。0 为不增加。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 5f)));
            _vaseMoraleCap = config.Bind("Morale", "VaseMoraleCap", game.VaseMoraleCap,
                new ConfigDescription(
                    "The limit on morale from vases, times the game's limit: 2 lets twice as many vases count. 1 is the game's own.\n" +
                    "花瓶带来的「心态」上限，为游戏原本的倍数：2 为可以计入两倍数量的花瓶。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0.5f, 5f)));
            _fullCraftingXp = config.Bind("Crafting", "FullCraftingXp", game.FullCraftingXp,
                "Recipes below your crafting level still give full proficiency. Off: the game gives less for recipes one to four levels below yours.\n" +
                "低于你生产等级的配方仍然给予全部「熟练度」。关闭后，比你低一到四级的配方给予的熟练度会减少，与游戏原本相同。");
            _pickupReach = config.Bind("Reach", "PickupReach", game.PickupReach,
                new ConfigDescription(
                    "How far away you can pick up dropped items, times the game's distance: 2 is twice as far. 1 is the game's own.\n" +
                    "拾取掉落物的距离，为游戏原本的倍数：2 为两倍远。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(1f, 5f)));

            All = new ConfigEntryBase[]
            {
                _neverDie, _rescueDeadline, _trustLoss, _mishapChance, _commissionChance, _aidPlatformChance,
                _thoughtFrequency, _moraleGains, _vaseMoraleCap, _fullCraftingXp, _pickupReach,
            }.Concat(_mishapsOff.Values).ToArray();
        }

        public ConfigEntryBase[] All { get; }

        public PeopleSettings Read() => new(
            _neverDie.Value,
            _rescueDeadline.Value,
            _trustLoss.Value,
            _mishapChance.Value,
            _mishapsOff.Where(pair => pair.Value.Value).Select(pair => pair.Key).ToList(),
            _commissionChance.Value,
            _aidPlatformChance.Value,
            _thoughtFrequency.Value,
            _moraleGains.Value,
            _vaseMoraleCap.Value,
            _fullCraftingXp.Value,
            _pickupReach.Value);

        private static ConfigEntry<bool> BindMishapOff(ConfigFile config, Mishap mishap, string english, string chinese) =>
            config.Bind("Strangers", $"No{mishap}Mishap", false,
                $"Strangers you support never suffer this mishap: {english}. Off: it can happen as in the game.\n" +
                $"你支援的陌生人不会遭遇这种灾祸：{chinese}。关闭后与游戏原本相同。");
    }
}
