using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class FrameClimateTests
{
    private sealed class CountingGame : IGameClimate
    {
        public int Heat = 5;
        public int Reads;

        public int DeviceHeatIn(int zone)
        {
            Reads++;

            return Heat + zone;
        }

        public bool? TakesDeviceHeat(int zone)
        {
            Reads++;

            return true;
        }

        public bool IsAcPoweredIn(int zone)
        {
            Reads++;

            return zone == Zones.Sunroom;
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
    public void A_pooled_temperature_read_dozens_of_times_a_frame_reads_each_pool_zone_once()
    {
        var pooled = new PooledClimate(new HeatPool(new PoolSettings(IndoorAndSunroom: true, Terrace: true, Basement: true)), Climate());

        for (var i = 0; i < 44; i++) pooled.DeviceHeat(Zones.Indoor, 3);

        Assert.Equal(3, _game.Reads);
    }
}
