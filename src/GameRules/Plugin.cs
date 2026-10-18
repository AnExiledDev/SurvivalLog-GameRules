using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using GameRules.Adapters;
using GameRules.Core;
using HarmonyLib;

namespace GameRules;

[BepInPlugin(Guid, "Game Rules", Version)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.gamerules";
    private const string Version = "0.1.0";

    public override void Load()
    {
        var log = new RulesLog(Log, Path.Combine(Paths.BepInExRootPath, "game-rules.log"));

        log.TrimIfLarge();

        var settings = BindRules();
        var pool = BindPool(log.Write);
        var harmony = new Harmony(Guid);

        log.Write($"===== Game Rules {Version} started");
        StartRestartCap(settings, log.Write);
        RulePatches.Apply(harmony, settings, log.Write);

        if (settings.ChangesSolar) SolarDetour.Apply(settings, log.Write);

        StartPool(harmony, pool, log.Write);
    }

    private RuleSettings BindRules()
    {
        var game = RuleSettings.Game;

        return new RuleSettings(
            Config.Bind("Restarts", "NoRestartLimit", game.NoRestartLimit,
                "Restarts never run out: however many you use, the game's full number stays available. Off: the game's limit.\n" +
                "重生次数不会用完：无论用了多少次，始终保留游戏原本的全部次数。关闭后使用游戏原本的次数上限。").Value,
            Config.Bind("Mold", "StopMoldSpread", game.StopMoldSpread,
                "Mold no longer spreads to the food next to it. Mold that is already there still clears as usual.\n" +
                "霉菌不再蔓延到旁边的食物。已有的霉菌仍会照常消退。").Value,
            Config.Bind("Solar", "SolarMultiplier", game.SolarMultiplier,
                new ConfigDescription(
                    "Solar panels make this many times the game's power. 1 is the game's own.\n" +
                    "太阳能板的发电量为游戏原本的这么多倍。1 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0.1f, 10f))).Value,
            Config.Bind("Solar", "BadWeatherOutput", game.BadWeatherSolar,
                new ConfigDescription(
                    "In storms and heavy rain the game's solar panels make nothing. This gives them a share of a cloudy day's power instead: 0.5 is half, 1 is a full cloudy day. 0 is the game's own.\n" +
                    "在暴雨等恶劣天气中，游戏原本的太阳能板不发电。此项让它们发出阴天发电量的一部分：0.5 为一半，1 为与阴天相同。0 为游戏原本的数值。",
                    new AcceptableValueRange<float>(0f, 1f))).Value,
            Config.Bind("Spoilage", "SpoilageSpeed", game.SpoilageSpeed,
                new ConfigDescription(
                    "How fast food spoils, times the game's speed: 0.5 is half as fast, 2 is twice as fast. 1 is the game's own. Fridges that keep food slower than the room keep their own speed. Food already lying somewhere picks up a new speed the next time it is moved or stored.\n" +
                    "食物腐败的速度，为游戏原本速度的倍数：0.5 为一半速度，2 为两倍速度。1 为游戏原本的数值。比房间保鲜更好的冰箱保持自己的速度。已经放好的食物在下次移动或存放时才会换成新的速度。",
                    new AcceptableValueRange<float>(0.1f, 5f))).Value);
    }

    private PoolSettings BindPool(System.Action<string> log)
    {
        var hadPoolSection = ClimateMove.HasPoolSection(Config);
        var game = PoolSettings.Default;
        var indoorAndSunroom = Config.Bind("Pool", "IndoorAndSunroom", game.IndoorAndSunroom,
            "Running AC units, heaters and stoves in the Indoor zone or the Sunroom warm both. Off: each warms only its own zone, as in the game.\n" +
            "室内或阳光房中运行的空调、电暖器和暖炉会同时温暖这两处。关闭后各自只温暖所在的区域，与游戏原本相同。");
        var terrace = Config.Bind("Pool", "Terrace", game.Terrace,
            "The Terrace joins Indoor's pool. The game gives the Terrace no device heat at all; on, it gets the whole pool's device heat, and devices on the Terrace warm the pool.\n" +
            "露台加入室内的共享范围。游戏本身不给露台任何设备热量；开启后露台获得整个共享范围的设备热量，露台上的设备也会温暖共享范围。");
        var basement = Config.Bind("Pool", "Basement", game.Basement,
            "The Basement joins Indoor's pool: its devices warm the pool and the pool's devices warm the Basement. Off: the Basement keeps its own heat, as in the game.\n" +
            "地下室加入室内的共享范围：地下室的设备温暖共享范围，共享范围的设备也温暖地下室。关闭后地下室保持自己的热量，与游戏原本相同。");

        ClimateMove.ImportOldPool(Config, hadPoolSection, new[] { indoorAndSunroom, terrace, basement }, log);

        return new PoolSettings(indoorAndSunroom.Value, terrace.Value, basement.Value);
    }

    private void StartRestartCap(RuleSettings settings, System.Action<string> log)
    {
        if (!settings.NoRestartLimit) return;

        RulesTicker.Log = log;
        RulesTicker.Checks.Add(new RestartCapField(new RestartKeeper(), log).Tick);
        AddComponent<RulesTicker>();
        log("restarts never run out");
    }

    private static void StartPool(Harmony harmony, PoolSettings pool, System.Action<string> log)
    {
        if (pool != PoolSettings.Default && ClimateMove.IsOldModInstalled())
        {
            log("Whole Floor Climate is still installed, so it keeps sharing the heat and this mod's Pool settings wait; remove WholeFloorClimate.dll to use them");

            return;
        }

        PoolPatches.Apply(harmony, new PooledClimate(new HeatPool(pool), new GameClimate()), log);
    }
}
