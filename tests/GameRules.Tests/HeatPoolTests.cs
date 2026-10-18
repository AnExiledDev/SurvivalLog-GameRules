using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class HeatPoolTests
{
    private const int Ac = 2;
    private const int Heater = 1;
    private const int Stove = 1;

    private static readonly PoolSettings Shared = new(IndoorAndSunroom: true, Terrace: false, Basement: false);
    private static readonly HeatPool SharedPool = new(Shared);
    private static readonly HeatPool Everything = new(new PoolSettings(IndoorAndSunroom: true, Terrace: true, Basement: true));
    private static readonly HeatPool Nothing = new(new PoolSettings(IndoorAndSunroom: false, Terrace: false, Basement: false));

    [Fact]
    public void A_heater_in_the_sunroom_warms_the_indoor_zone_by_default()
    {
        var house = new FakeHouse().Place(Zones.Sunroom, Heater);

        Assert.Equal(house.Weather[Zones.Indoor] + Heater, house.Temperature(SharedPool, Zones.Indoor));
        Assert.Equal(house.Weather[Zones.Sunroom] + Heater, house.Temperature(SharedPool, Zones.Sunroom));
    }

    [Fact]
    public void An_ac_indoors_warms_the_sunroom_by_default()
    {
        var house = new FakeHouse().Place(Zones.Indoor, Ac);

        Assert.Equal(house.Weather[Zones.Sunroom] + Ac, house.Temperature(SharedPool, Zones.Sunroom));
    }

    [Fact]
    public void Each_device_counts_once_in_every_pool_zone()
    {
        var house = new FakeHouse().Place(Zones.Indoor, Ac).Place(Zones.Sunroom, Heater).Place(Zones.Sunroom, Stove);

        Assert.Equal(house.Weather[Zones.Indoor] + Ac + Heater + Stove, house.Temperature(SharedPool, Zones.Indoor));
        Assert.Equal(house.Weather[Zones.Sunroom] + Ac + Heater + Stove, house.Temperature(SharedPool, Zones.Sunroom));
    }

    [Fact]
    public void Moving_a_device_between_pool_zones_changes_no_temperature()
    {
        var everyArrangement = Everything.Members.Select(zone => new FakeHouse().Place(zone, Ac).Place(Zones.Indoor, Heater)).ToList();

        foreach (var zone in FakeHouse.AllZones)
        {
            var temperatures = everyArrangement.Select(house => house.Temperature(Everything, zone)).Distinct();

            Assert.Single(temperatures);
        }
    }

    [Fact]
    public void With_every_option_off_every_zone_keeps_the_games_temperature()
    {
        var house = new FakeHouse();

        foreach (var zone in FakeHouse.AllZones) house.Place(zone, Ac);

        Assert.False(Nothing.IsActive);

        foreach (var zone in FakeHouse.AllZones) Assert.Equal(house.GameTemperature(zone), house.Temperature(Nothing, zone));
    }

    [Fact]
    public void Cold_storage_never_shares_heat_even_with_every_zone_pooled()
    {
        var chilled = new FakeHouse().Place(Zones.Indoor, Ac).Place(Zones.Sunroom, Heater);
        var heaterInColdStorage = new FakeHouse().Place(Zones.ColdStorage, Ac);

        Assert.Equal(chilled.GameTemperature(Zones.ColdStorage), chilled.Temperature(Everything, Zones.ColdStorage));
        Assert.All(Everything.Members, zone => Assert.Equal(0, heaterInColdStorage.Gain(Everything, zone)));
        Assert.DoesNotContain(Zones.ColdStorage, Everything.Members);
    }

    [Fact]
    public void Terrace_and_basement_keep_the_games_rules_by_default()
    {
        var house = new FakeHouse().Place(Zones.Indoor, Ac).Place(Zones.Terrace, Heater).Place(Zones.Basement, Stove);

        Assert.Equal(house.GameTemperature(Zones.Terrace), house.Temperature(SharedPool, Zones.Terrace));
        Assert.Equal(house.GameTemperature(Zones.Basement), house.Temperature(SharedPool, Zones.Basement));
        Assert.Equal(house.Weather[Zones.Indoor] + Ac, house.Temperature(SharedPool, Zones.Indoor));
    }

    [Fact]
    public void A_pooled_terrace_gets_device_heat_the_game_never_gives_it()
    {
        var house = new FakeHouse().Place(Zones.Indoor, Ac).Place(Zones.Sunroom, Heater);
        var withTerrace = new HeatPool(Shared with { Terrace = true });

        Assert.Equal(house.Weather[Zones.Terrace], house.GameTemperature(Zones.Terrace));
        Assert.Equal(house.Weather[Zones.Terrace] + Ac + Heater, house.Temperature(withTerrace, Zones.Terrace));
    }

    [Fact]
    public void A_heater_on_a_pooled_terrace_warms_the_house_and_counts_once_on_the_terrace()
    {
        var house = new FakeHouse().Place(Zones.Terrace, Heater).Place(Zones.Indoor, Ac);
        var withTerrace = new HeatPool(Shared with { Terrace = true });

        Assert.Equal(house.Weather[Zones.Indoor] + Ac + Heater, house.Temperature(withTerrace, Zones.Indoor));
        Assert.Equal(house.Weather[Zones.Sunroom] + Ac + Heater, house.Temperature(withTerrace, Zones.Sunroom));
        Assert.Equal(house.Weather[Zones.Terrace] + Ac + Heater, house.Temperature(withTerrace, Zones.Terrace));
    }

    [Fact]
    public void A_pooled_basement_shares_heat_both_ways()
    {
        var house = new FakeHouse().Place(Zones.Basement, Heater).Place(Zones.Sunroom, Ac);
        var withBasement = new HeatPool(Shared with { Basement = true });

        Assert.Equal(house.Weather[Zones.Basement] + Heater + Ac, house.Temperature(withBasement, Zones.Basement));
        Assert.Equal(house.Weather[Zones.Indoor] + Heater + Ac, house.Temperature(withBasement, Zones.Indoor));
    }

    [Fact]
    public void Terrace_and_basement_join_independently_of_each_other()
    {
        var house = new FakeHouse().Place(Zones.Indoor, Ac).Place(Zones.Terrace, Heater);
        var withBasement = new HeatPool(Shared with { Basement = true });

        Assert.Equal(house.GameTemperature(Zones.Terrace), house.Temperature(withBasement, Zones.Terrace));
        Assert.Equal(house.Weather[Zones.Basement] + Ac, house.Temperature(withBasement, Zones.Basement));
    }

    [Fact]
    public void Without_the_sunroom_a_joining_zone_still_pools_with_indoor()
    {
        var house = new FakeHouse().Place(Zones.Basement, Heater).Place(Zones.Sunroom, Ac);
        var basementOnly = new HeatPool(new PoolSettings(IndoorAndSunroom: false, Terrace: false, Basement: true));

        Assert.Equal(house.Weather[Zones.Indoor] + Heater, house.Temperature(basementOnly, Zones.Indoor));
        Assert.Equal(house.GameTemperature(Zones.Sunroom), house.Temperature(basementOnly, Zones.Sunroom));
    }

    [Fact]
    public void A_zone_the_game_already_warms_with_devices_gets_no_second_helping()
    {
        Assert.Equal(0, Everything.ExtraWarmth(Zones.Indoor, gameWarmsWithDevices: true, _ => Ac));
    }

    [Fact]
    public void A_zone_the_game_does_not_know_keeps_its_numbers()
    {
        const int unknownZone = 9;

        Assert.Equal(Heater, Everything.DeviceHeatFor(unknownZone, Heater, _ => Ac));
        Assert.Equal(0, Everything.ExtraWarmth(unknownZone, gameWarmsWithDevices: false, _ => Ac));
    }
}
