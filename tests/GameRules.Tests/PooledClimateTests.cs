using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class PooledClimateTests
{
    private const int Ac = 2;
    private const int Heater = 1;

    private static readonly PoolSettings Shared = new(IndoorAndSunroom: true, Terrace: false, Basement: false);
    private static readonly HeatPool SharedPool = new(Shared);
    private static readonly HeatPool Everything = new(new PoolSettings(IndoorAndSunroom: true, Terrace: true, Basement: true));
    private static readonly HeatPool Nothing = new(new PoolSettings(IndoorAndSunroom: false, Terrace: false, Basement: false));
    private static readonly HeatPool WithTerrace = new(Shared with { Terrace = true });

    [Fact]
    public void An_ac_running_in_the_sunroom_counts_as_powered_indoors_by_default()
    {
        var house = new FakeHouse().PlaceAc(Zones.Sunroom, Ac);

        Assert.False(house.IsAcPoweredIn(Zones.Indoor));
        Assert.True(house.IsAcPowered(SharedPool, Zones.Indoor));
    }

    [Fact]
    public void An_ac_running_indoors_counts_as_powered_in_the_sunroom_by_default()
    {
        var house = new FakeHouse().PlaceAc(Zones.Indoor, Ac);

        Assert.True(house.IsAcPowered(SharedPool, Zones.Sunroom));
    }

    [Fact]
    public void A_heater_is_not_an_ac_for_any_pool_zone()
    {
        var house = new FakeHouse().Place(Zones.Sunroom, Heater);

        Assert.All(Everything.Members, zone => Assert.False(house.IsAcPowered(Everything, zone)));
    }

    [Fact]
    public void Zones_outside_the_pool_keep_the_games_ac_answer()
    {
        var acIndoors = new FakeHouse().PlaceAc(Zones.Indoor, Ac);
        var acInTheBasement = new FakeHouse().PlaceAc(Zones.Basement, Ac);

        Assert.False(acIndoors.IsAcPowered(SharedPool, Zones.Basement));
        Assert.False(acIndoors.IsAcPowered(SharedPool, Zones.Terrace));
        Assert.True(acInTheBasement.IsAcPowered(SharedPool, Zones.Basement));
        Assert.False(acInTheBasement.IsAcPowered(SharedPool, Zones.Indoor));
    }

    [Fact]
    public void Cold_storage_never_borrows_or_lends_an_ac_even_with_every_zone_pooled()
    {
        var acIndoors = new FakeHouse().PlaceAc(Zones.Indoor, Ac);
        var acInColdStorage = new FakeHouse().PlaceAc(Zones.ColdStorage, Ac);

        Assert.False(acIndoors.IsAcPowered(Everything, Zones.ColdStorage));
        Assert.All(Everything.Members, zone => Assert.False(acInColdStorage.IsAcPowered(Everything, zone)));
    }

    [Fact]
    public void With_every_option_off_every_ac_answer_is_the_games()
    {
        var house = new FakeHouse().PlaceAc(Zones.Sunroom, Ac).PlaceAc(Zones.Basement, Ac);

        Assert.All(FakeHouse.AllZones, zone => Assert.Equal(house.IsAcPoweredIn(zone), house.IsAcPowered(Nothing, zone)));
    }

    [Fact]
    public void A_pooled_terrace_counts_an_ac_running_indoors()
    {
        var house = new FakeHouse().PlaceAc(Zones.Indoor, Ac);

        Assert.True(house.IsAcPowered(WithTerrace, Zones.Terrace));
    }

    [Fact]
    public void A_pool_zone_whose_config_takes_device_heat_gets_the_pool_once()
    {
        var house = new FakeHouse().PlaceAc(Zones.Indoor, Ac).Place(Zones.Sunroom, Heater);

        house.DeviceHeatFlags[Zones.Terrace] = true;

        Assert.Equal(house.Weather[Zones.Terrace] + Ac + Heater, house.Temperature(WithTerrace, Zones.Terrace));
    }

    [Fact]
    public void A_pool_zone_the_game_has_no_config_for_keeps_the_games_temperature()
    {
        var house = new FakeHouse().PlaceAc(Zones.Indoor, Ac).Place(Zones.Sunroom, Heater);

        house.DeviceHeatFlags.Remove(Zones.Terrace);

        Assert.Equal(house.GameTemperature(Zones.Terrace), house.Temperature(WithTerrace, Zones.Terrace));
    }
}
