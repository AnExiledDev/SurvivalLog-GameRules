using System.Collections.Generic;
using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class OpenAreasTests
{
    private const int WageSlave = 1;
    private const int Student = 2;
    private const int WarehouseManager = 3;
    private const int Veteran = 4;
    private const int FindSecondFloorKey = 30028;
    private const int ClearTheCellar = 40420;
    private const int ColdStorageKey = 61201;
    private const int GarageKey = 61210;
    private const int ColdWaveStrikes = 20200;
    private const bool AfterDisaster = false;
    private const bool BeforeDisaster = true;

    [Theory]
    [InlineData(WageSlave)]
    [InlineData(Student)]
    public void A_house_run_opens_both_floors_and_clears_the_stairs_and_cellar(int role)
    {
        var tags = new HashSet<string> { "TAG_STAIRS_BROKEN", "TAG_CELLAR_BLOCKED", "SOME_STORY_TAG" };

        var change = OpenAreas.Plan(role, AfterDisaster, tags.Contains);

        Assert.Equal(new[] { OpenAreas.SecondFloor, OpenAreas.Basement }, change.Add);
        Assert.Equal(new[] { "TAG_STAIRS_BROKEN", "TAG_CELLAR_BLOCKED" }, change.Remove);
    }

    [Fact]
    public void A_warehouse_run_opens_the_second_floor_and_every_locked_room()
    {
        var change = OpenAreas.Plan(WarehouseManager, AfterDisaster, _ => false);

        Assert.Equal(
            new[] { OpenAreas.SecondFloor, OpenAreas.ColdStorage, OpenAreas.RightHut, OpenAreas.LeftHut, OpenAreas.Garage },
            change.Add);
        Assert.Empty(change.Remove);
    }

    [Fact]
    public void Only_the_areas_still_locked_are_opened()
    {
        var tags = new HashSet<string> { OpenAreas.SecondFloor, OpenAreas.Garage };

        var change = OpenAreas.Plan(WarehouseManager, AfterDisaster, tags.Contains);

        Assert.Equal(new[] { OpenAreas.ColdStorage, OpenAreas.RightHut, OpenAreas.LeftHut }, change.Add);
    }

    [Fact]
    public void A_run_with_everything_open_is_left_alone()
    {
        var tags = new HashSet<string> { OpenAreas.SecondFloor, OpenAreas.Basement };

        Assert.True(OpenAreas.Plan(WageSlave, AfterDisaster, tags.Contains).IsEmpty);
    }

    [Fact]
    public void A_role_with_no_locked_areas_gets_nothing()
    {
        Assert.True(OpenAreas.Plan(Veteran, AfterDisaster, _ => false).IsEmpty);
    }

    [Fact]
    public void An_opening_event_is_skipped_once_its_area_is_open()
    {
        var tags = new HashSet<string> { OpenAreas.SecondFloor, OpenAreas.ColdStorage };

        Assert.True(OpenAreas.SkipsEvent(FindSecondFloorKey, tags.Contains));
        Assert.True(OpenAreas.SkipsEvent(ColdStorageKey, tags.Contains));
    }

    [Fact]
    public void An_opening_event_still_happens_while_its_area_is_locked()
    {
        var tags = new HashSet<string> { OpenAreas.SecondFloor };

        Assert.False(OpenAreas.SkipsEvent(ClearTheCellar, tags.Contains));
        Assert.False(OpenAreas.SkipsEvent(GarageKey, tags.Contains));
    }

    [Fact]
    public void Other_events_are_never_skipped()
    {
        Assert.False(OpenAreas.SkipsEvent(ColdWaveStrikes, _ => true));
    }

    [Theory]
    [InlineData(WageSlave)]
    [InlineData(WarehouseManager)]
    public void Nothing_opens_before_the_disaster(int role)
    {
        var tags = new HashSet<string> { "TAG_STAIRS_BROKEN", "TAG_CELLAR_BLOCKED" };

        Assert.True(OpenAreas.Plan(role, BeforeDisaster, tags.Contains).IsEmpty);
    }
}
