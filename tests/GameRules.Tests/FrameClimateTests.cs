using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

/// <summary>
/// The read counts here are the cost the cache exists to save: the game is asked dozens of times a frame, and each
/// question it answers costs the frame time the mod is being fixed for. So the counts are the behaviour under test.
/// </summary>
public sealed class FrameClimateTests
{
    private sealed class CountingGame : IGameClimate
    {
        public int Heat = 5;
        public int Reads;
        public int TakesReads;
        public int AcReads;
        public int GameReads;
        public bool TakesHeat = true;
        public int AcZone = Zones.Sunroom;
        public LoadedGame Game = new(World: 100, Config: 200);

        public int DeviceHeatIn(int zone)
        {
            Reads++;

            return Heat + zone;
        }

        public bool? TakesDeviceHeat(int zone)
        {
            Reads++;
            TakesReads++;

            return TakesHeat;
        }

        public bool IsAcPoweredIn(int zone)
        {
            Reads++;
            AcReads++;

            return zone == AcZone;
        }

        public LoadedGame CurrentGame()
        {
            GameReads++;

            return Game;
        }
    }

    private readonly CountingGame _game = new();
    private int _frame = 1;

    private FrameClimate Climate() => new(_game, () => _frame);

    [Fact]
    public void A_zone_asked_many_times_in_one_frame_is_read_from_the_game_once()
    {
        var climate = Climate();

        for (var i = 0; i < 40; i++) climate.DeviceHeatIn(Zones.Indoor);

        Assert.Equal(1, _game.Reads);
    }

    [Fact]
    public void Each_zone_is_read_on_its_own()
    {
        var climate = Climate();

        Assert.Equal(5 + Zones.Indoor, climate.DeviceHeatIn(Zones.Indoor));
        Assert.Equal(5 + Zones.Sunroom, climate.DeviceHeatIn(Zones.Sunroom));
        Assert.True(climate.IsAcPoweredIn(Zones.Sunroom));
        Assert.False(climate.IsAcPoweredIn(Zones.Indoor));
    }

    [Fact]
    public void A_heater_switched_on_shows_from_the_next_frame()
    {
        var climate = Climate();

        climate.DeviceHeatIn(Zones.Indoor);
        _game.Heat = 50;

        Assert.Equal(5 + Zones.Indoor, climate.DeviceHeatIn(Zones.Indoor));

        _frame++;

        Assert.Equal(50 + Zones.Indoor, climate.DeviceHeatIn(Zones.Indoor));
    }

    [Fact]
    public void An_ac_is_read_once_a_frame_and_switching_it_shows_from_the_next_frame()
    {
        var climate = Climate();

        for (var i = 0; i < 20; i++) Assert.True(climate.IsAcPoweredIn(Zones.Sunroom));

        Assert.Equal(1, _game.AcReads);

        _game.AcZone = Zones.Indoor;

        Assert.True(climate.IsAcPoweredIn(Zones.Sunroom));

        _frame++;

        Assert.False(climate.IsAcPoweredIn(Zones.Sunroom));
        Assert.Equal(2, _game.AcReads);
    }

    [Fact]
    public void A_pooled_temperature_read_dozens_of_times_a_frame_reads_each_pool_zone_once()
    {
        var pooled = new PooledClimate(new HeatPool(new PoolSettings(IndoorAndSunroom: true, Terrace: true, Basement: true)), Climate());

        for (var i = 0; i < 44; i++) pooled.DeviceHeat(Zones.Indoor, 3);

        Assert.Equal(3, _game.Reads);
    }

    [Fact]
    public void Whether_a_zone_takes_device_heat_is_read_once_for_the_same_save_and_config_however_many_frames_pass()
    {
        var climate = Climate();

        for (var frame = 0; frame < 30; frame++)
        {
            _frame++;

            Assert.True(climate.TakesDeviceHeat(Zones.Indoor));
        }

        Assert.Equal(1, _game.TakesReads);
    }

    [Fact]
    public void Loading_a_save_reads_whether_a_zone_takes_device_heat_again()
    {
        var climate = Climate();

        Assert.True(climate.TakesDeviceHeat(Zones.Indoor));

        _game.TakesHeat = false;
        _game.Game = _game.Game with { World = 101 };
        _frame++;

        Assert.False(climate.TakesDeviceHeat(Zones.Indoor));
        Assert.Equal(2, _game.TakesReads);
    }

    [Fact]
    public void Reloading_the_configs_reads_whether_a_zone_takes_device_heat_again()
    {
        var climate = Climate();

        Assert.True(climate.TakesDeviceHeat(Zones.Indoor));

        _game.TakesHeat = false;
        _game.Game = _game.Game with { Config = 201 };
        _frame++;

        Assert.False(climate.TakesDeviceHeat(Zones.Indoor));
        Assert.Equal(2, _game.TakesReads);
    }

    [Fact]
    public void Going_back_to_the_title_screen_and_loading_again_reads_it_again_too()
    {
        var climate = Climate();

        climate.TakesDeviceHeat(Zones.Indoor);

        _game.Game = default;
        _frame++;
        climate.TakesDeviceHeat(Zones.Indoor);

        _game.Game = new LoadedGame(World: 300, Config: 200);
        _frame++;
        climate.TakesDeviceHeat(Zones.Indoor);

        Assert.Equal(3, _game.TakesReads);
    }

    [Fact]
    public void Which_game_is_loaded_is_read_once_a_frame_not_once_a_question()
    {
        var climate = Climate();

        for (var i = 0; i < 40; i++)
        {
            climate.DeviceHeatIn(Zones.Indoor);
            climate.TakesDeviceHeat(Zones.Indoor);
            climate.IsAcPoweredIn(Zones.Indoor);
        }

        Assert.Equal(1, _game.GameReads);

        _frame++;
        climate.DeviceHeatIn(Zones.Indoor);

        Assert.Equal(2, _game.GameReads);
    }

    public static TheoryData<bool, bool, bool> EveryPool()
    {
        var pools = new TheoryData<bool, bool, bool>();

        foreach (var sunroom in new[] { false, true })
        foreach (var terrace in new[] { false, true })
        foreach (var basement in new[] { false, true })
            pools.Add(sunroom, terrace, basement);

        return pools;
    }

    [Theory]
    [MemberData(nameof(EveryPool))]
    public void Every_pooled_answer_through_the_cache_is_the_same_as_asking_the_game_each_time(bool sunroom, bool terrace, bool basement)
    {
        var house = new FakeHouse()
            .Place(Zones.Indoor, 2).Place(Zones.Sunroom, 1).Place(Zones.Terrace, 3).Place(Zones.Basement, 4).Place(Zones.ColdStorage, 6)
            .PlaceAc(Zones.Sunroom, 2);
        var pool = new HeatPool(new PoolSettings(sunroom, terrace, basement));
        var direct = new PooledClimate(pool, house);
        var cached = new PooledClimate(pool, new FrameClimate(house, () => 7));

        foreach (var zone in FakeHouse.AllZones)
        {
            // Asked twice, so the second answer comes from the cache.
            for (var ask = 0; ask < 2; ask++)
            {
                var gameHeat = house.DeviceHeatIn(zone);

                Assert.Equal(direct.DeviceHeat(zone, gameHeat), cached.DeviceHeat(zone, gameHeat));
                Assert.Equal(direct.Temperature(zone, house.Weather[zone]), cached.Temperature(zone, house.Weather[zone]));
                Assert.Equal(direct.IsAcPowered(zone, house.IsAcPoweredIn(zone)), cached.IsAcPowered(zone, house.IsAcPoweredIn(zone)));
            }
        }
    }
}
