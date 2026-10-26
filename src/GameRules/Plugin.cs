using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using GameRules.Adapters;
using GameRules.Core;
using GameRules.Settings;
using HarmonyLib;

namespace GameRules;

[BepInPlugin(Guid, "Game Rules", Version)]
[BepInDependency(OptionalLive.ToolkitGuid, BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.gamerules";
    private const string Version = "0.6.1";

    public override void Load()
    {
        if (!OptionalLive.IsToolkitInstalled) Log.LogInfo(OptionalLive.MissingNote);

        var log = new RulesLog(Log, Path.Combine(Paths.BepInExRootPath, "game-rules.log"));

        log.TrimIfLarge();

        var rules = BindRules();
        var visitors = BindVisitors();
        var weather = BindWeather();
        var pool = BindPool(log.Write);
        var openAreas = BindAreas();
        var harmony = new Harmony(Guid);
        var startRules = rules.Read();

        log.Write($"===== Game Rules {Version} started");
        StartRestartCap(startRules, log.Write);

        OptionalLive.Watch(() =>
        {
            var settings = rules.Read();

            RulePatches.Use(harmony, settings, log.Write);
            SolarDetour.Use(settings, log.Write);
        }, rules.StopMoldSpread, rules.SolarMultiplier, rules.BadWeatherOutput, rules.SpoilageSpeed);

        OptionalLive.Watch(() => VisitorPatches.Use(harmony, new VisitorBlock(visitors.NoDoorSmasher.Value, visitors.NoDebtCollector.Value), log.Write),
            visitors.NoDoorSmasher, visitors.NoDebtCollector);

        OptionalLive.Watch(() => UsePool(harmony, pool.Read(), log.Write), pool.IndoorAndSunroom, pool.Terrace, pool.Basement);

        StartOpenAreas(harmony, openAreas, log.Write);
        StartTables(startRules, weather, log.Write);
        StartGroups(harmony, log.Write);
        StartTicker(log.Write);
    }

    /// <summary>Each group binds its own settings; their GlobalSetting changes all go through one table writer.</summary>
    private void StartGroups(Harmony harmony, System.Action<string> log)
    {
        var table = new TableChanges();

        FoodAndHealthRules.Start(Config, harmony, table, log);
        HouseAndZombieRules.Start(Config, harmony, table, log);
        PeopleAndMoraleRules.Start(Config, harmony, table, log);
        RulesTicker.Checks.Add(new GlobalTable(table, log).Tick);
    }

    private RuleEntries BindRules()
    {
        var game = RuleSettings.Game;

        return new RuleEntries(
            Config.Bind("Restarts", "NoRestartLimit", game.NoRestartLimit,
                new ConfigDescription(
                    "Restarts never run out: however many you use, the game's full number stays available. Off: the game's limit. Takes effect at the next game start.\n" +
                    "重生次数不会用完：无论用了多少次，始终保留游戏原本的全部次数。关闭后使用游戏原本的次数上限。下次启动游戏时生效。",
                    null, "ModManager.Restart")),
            Config.Bind("Mold", "StopMoldSpread", game.StopMoldSpread,
                "Mold no longer spreads to the food next to it. Mold that is already there still clears as usual.\n" +
                "霉菌不再蔓延到旁边的食物。已有的霉菌仍会照常消退。"),
            Config.Bind("Solar", "SolarMultiplier", game.SolarMultiplier,
                new ConfigDescription(
                    "Solar panels make this many times the game's power. 1 is the game's own.\n" +
                    "太阳能板的发电量为游戏原本的这么多倍。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0.1f, 10f))),
            Config.Bind("Solar", "BadWeatherOutput", game.BadWeatherSolar,
                new ConfigDescription(
                    "In storms and heavy rain the game's solar panels make nothing. This gives them a share of a cloudy day's power instead: 0.5 is half, 1 is a full cloudy day. 0 is the game's own.\n" +
                    "在暴雨等恶劣天气中，游戏原本的太阳能板不发电。此项让它们发出阴天发电量的一部分：0.5 为一半，1 为与阴天相同。0 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 1f))),
            Config.Bind("Spoilage", "SpoilageSpeed", game.SpoilageSpeed,
                new ConfigDescription(
                    "How fast food spoils, times the game's speed: 0.5 is half as fast, 2 is twice as fast. 1 is the game's own. Fridges that keep food slower than the room keep their own speed. Food already lying somewhere picks up a new speed the next time it is moved or stored.\n" +
                    "食物腐败的速度，为游戏原本速度的倍数：0.5 为一半速度，2 为两倍速度。1 为游戏原本的数值。比房间保鲜更好的冰箱保持自己的速度。已经放好的食物在下次移动或存放时才会换成新的速度。",
                    new AcceptableValueRange<float>(0.1f, 5f))),
            Config.Bind("Power", "BatteryMultiplier", game.BatteryMultiplier,
                new ConfigDescription(
                    "Batteries hold this many times the game's power: 2 holds twice as much. 1 is the game's own. Takes effect at the next game start.\n" +
                    "蓄电池的容量为游戏原本的这么多倍：2 为两倍。1 为游戏原本的数值。下次启动游戏时生效。",
                    new AcceptableValueRange<float>(0.1f, 10f), "ModManager.Restart")));
    }

    private VisitorEntries BindVisitors()
    {
        var game = VisitorBlock.Game;

        return new VisitorEntries(
            Config.Bind("Visitors", "NoDoorSmasher", game.NoDoorSmasher,
                "The angry neighbour never comes pounding on your door at night (Midnight Door Pounding).\n" +
                "愤怒的邻居不会在深夜来砸门（「深夜砸门」）。"),
            Config.Bind("Visitors", "NoDebtCollector", game.NoDebtCollector,
                "The loan shark never comes to collect: no warning text, no Midnight Visitors, no revenge nights. A loan you take is yours to keep. Best turned on before the first visit.\n" +
                "放贷人不会上门催收：没有警告短信，没有「深夜来客」，也没有报复的夜晚。借来的钱不用还。最好在第一次上门前开启。"));
    }

    private bool BindAreas() =>
        Config.Bind("Areas", "StartWithAllAreasOpen", false, new ConfigDescription(
            "Every area you would open with a key or a repair is open as soon as the disaster hits: the second floor and basement for the Wage Slave and the Student; the second floor, cold storage, garage and both side rooms for the Warehouse Manager. You can walk there and build there right away, and the story events that would open them are skipped, along with their rewards. Before the disaster they stay locked. Also opens them in a save already past the disaster. Takes effect at the next game start.\n" +
            "所有原本需要钥匙或修理才能打开的区域在灾难发生时立即打开：打工仔和大学生的二楼和地下室；仓库管理员的二楼、冷库、车库和两侧的小屋。可以直接走过去并在那里建造，原本用来打开这些区域的剧情事件及其奖励会被跳过。灾难前这些区域仍然锁着。对已经过了灾难的存档同样生效。下次启动游戏时生效。",
            null, "ModManager.Restart")).Value;

    private WeatherSettings BindWeather()
    {
        var game = WeatherSettings.Game;
        var randomWeather = Config.Bind("Weather", "RandomWeather", game.RandomWeather, new ConfigDescription(
            "Rolls new weather for the 100-day story from the chances below, in place of the game's fixed weather. Cold waves and the story's fixed-weather days stay as in the game, and the difficulty still makes weather milder or harsher. Endless mode is not changed. Takes effect at the next game start, from the first hour played; the forecast follows it.\n" +
            "按下方的概率为 100 天剧情重新生成天气，取代游戏固定的天气。寒潮和剧情固定天气的日子与游戏原本相同，难度仍会让天气变好或变差。无尽模式不受影响。下次启动游戏时生效，从进入游戏后的第一个小时起，天气预报也会随之改变。",
            null, "ModManager.Restart")).Value;
        var seed = Config.Bind("Weather", "WeatherSeed", game.Seed, new ConfigDescription(
            "Any whole number. The same number always rolls the same weather, so a reload keeps it; change it for different weather. Takes effect at the next game start.\n" +
            "任意整数。相同的数字总是生成相同的天气，所以读档后保持不变；换一个数字即可得到不同的天气。下次启动游戏时生效。",
            null, "ModManager.Restart", "ModManager.ShowWhen=RandomWeather")).Value;

        return new WeatherSettings(randomWeather, seed, new[]
        {
            Chance(WeatherSettings.Clear, "ClearChance", "Clear", "晴天", game),
            Chance(WeatherSettings.Cloudy, "CloudyChance", "Cloudy", "阴天", game),
            Chance(WeatherSettings.Rain, "RainChance", "Rainy", "雨天", game),
            Chance(WeatherSettings.HeavyRain, "HeavyRainChance", "Heavy Rain", "暴雨", game),
            Chance(WeatherSettings.ContinuousRain, "ContinuousRainChance", "Continuous Rain", "持续阴雨", game),
            Chance(WeatherSettings.Smog, "SmogChance", "Heavy Smog", "大雾霾天", game),
        });
    }

    /// <summary>One weather's share. The defaults are the game's own mix once cold waves are set aside.</summary>
    private WeatherChance Chance(int weather, string key, string english, string chinese, WeatherSettings game)
    {
        var gameChance = game.Chances.First(chance => chance.Weather == weather).Chance;

        return new WeatherChance(weather, Config.Bind("Weather", key, gameChance,
            new ConfigDescription(
                $"The chance of {english} with RandomWeather on. The chances are shares, so they need not add up to 100; 0 means never. Takes effect at the next game start.\n" +
                $"开启 RandomWeather 后「{chinese}」的概率。各项概率按比例分配，加起来不必等于 100；0 表示从不出现。下次启动游戏时生效。",
                new AcceptableValueRange<float>(0f, 100f), "ModManager.Restart", "ModManager.ShowWhen=RandomWeather")).Value);
    }

    private PoolEntries BindPool(System.Action<string> log)
    {
        var hadPoolSection = ClimateMove.HasClimateSection(Config);
        var game = PoolSettings.Default;
        var indoorAndSunroom = Config.Bind("Climate", "IndoorAndSunroom", game.IndoorAndSunroom,
            "Running AC units, heaters and stoves in the Indoor zone or the Sunroom warm both. Off: each warms only its own zone, as in the game.\n" +
            "室内或阳光房中运行的空调、电暖器和暖炉会同时温暖这两处。关闭后各自只温暖所在的区域，与游戏原本相同。");
        var terrace = Config.Bind("Climate", "Terrace", game.Terrace,
            "The Terrace shares heat with the Indoor zone. The game gives the Terrace no device heat at all; on, it gets the device heat of every zone sharing with Indoor, and devices on the Terrace warm those zones too.\n" +
            "露台加入室内的共享范围。游戏本身不给露台任何设备热量；开启后露台获得整个共享范围的设备热量，露台上的设备也会温暖共享范围。");
        var basement = Config.Bind("Climate", "Basement", game.Basement,
            "The Basement shares heat with the Indoor zone: its devices warm the zones sharing with Indoor, and their devices warm the Basement. Off: the Basement keeps its own heat, as in the game.\n" +
            "地下室加入室内的共享范围：地下室的设备温暖共享范围，共享范围的设备也温暖地下室。关闭后地下室保持自己的热量，与游戏原本相同。");

        ClimateMove.ImportOldPool(Config, hadPoolSection, new[] { indoorAndSunroom, terrace, basement }, log);

        return new PoolEntries(indoorAndSunroom, terrace, basement);
    }

    private void StartRestartCap(RuleSettings settings, System.Action<string> log)
    {
        if (!settings.NoRestartLimit) return;

        RulesTicker.Checks.Add(new RestartCapField(new RestartKeeper(), log).Tick);
        log("restarts never run out");
    }

    private static void StartTables(RuleSettings settings, WeatherSettings weather, System.Action<string> log)
    {
        if (weather.IsActive) RulesTicker.Checks.Add(new WeatherTable(weather, log).Tick);

        if (settings.ChangesBattery) RulesTicker.Checks.Add(new BatteryTable(settings.BatteryMultiplier, log).Tick);
    }

    /// <summary>One Unity object runs every check, so it is only added when a setting needs one.</summary>
    private void StartTicker(System.Action<string> log)
    {
        if (RulesTicker.Checks.Count == 0) return;

        RulesTicker.Log = log;
        AddComponent<RulesTicker>();
    }

    private static void StartOpenAreas(Harmony harmony, bool openAreas, System.Action<string> log)
    {
        if (!openAreas) return;

        AreaPatches.Apply(harmony, log);
        RulesTicker.Checks.Add(new AreaOpener(log).Tick);
        log("every locked area opens once the disaster has hit");
    }

    private static void UsePool(Harmony harmony, PoolSettings pool, System.Action<string> log)
    {
        if (pool != PoolSettings.Default && ClimateMove.IsOldModInstalled())
        {
            log("Whole Floor Climate is still installed, so it keeps sharing the heat and this mod's Climate settings wait; remove WholeFloorClimate.dll to use them");
            pool = PoolSettings.Default;
        }

        PoolPatches.Use(harmony, new PooledClimate(new HeatPool(pool), new FrameClimate(new GameClimate(), () => UnityEngine.Time.frameCount)), log);
    }

    private sealed record RuleEntries(
        ConfigEntry<bool> NoRestartLimit,
        ConfigEntry<bool> StopMoldSpread,
        ConfigEntry<float> SolarMultiplier,
        ConfigEntry<float> BadWeatherOutput,
        ConfigEntry<float> SpoilageSpeed,
        ConfigEntry<float> BatteryMultiplier)
    {
        public RuleSettings Read() => new(NoRestartLimit.Value, StopMoldSpread.Value, SolarMultiplier.Value, BadWeatherOutput.Value, SpoilageSpeed.Value, BatteryMultiplier.Value);
    }

    private sealed record VisitorEntries(ConfigEntry<bool> NoDoorSmasher, ConfigEntry<bool> NoDebtCollector);

    private sealed record PoolEntries(ConfigEntry<bool> IndoorAndSunroom, ConfigEntry<bool> Terrace, ConfigEntry<bool> Basement)
    {
        public PoolSettings Read() => new(IndoorAndSunroom.Value, Terrace.Value, Basement.Value);
    }
}
