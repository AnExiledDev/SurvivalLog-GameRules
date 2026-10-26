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
    private const int RepairTheStairs = 51502;
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

    /// <summary>The event hook reads the save's tags only for an opening event; that is only safe if no other event can be skipped.</summary>
    [Theory]
    [InlineData(FindSecondFloorKey)]
    [InlineData(RepairTheStairs)]
    [InlineData(ClearTheCellar)]
    [InlineData(ColdStorageKey)]
    [InlineData(GarageKey)]
    [InlineData(ColdWaveStrikes)]
    [InlineData(0)]
    public void Only_an_event_that_opens_an_area_can_ever_be_skipped(int eventId)
    {
        Assert.Equal(OpenAreas.SkipsEvent(eventId, _ => true), OpenAreas.OpensArea(eventId));
    }

    [Fact]
    public void An_opening_event_is_one_that_opens_an_area_and_a_cold_wave_is_not()
    {
        Assert.True(OpenAreas.OpensArea(FindSecondFloorKey));
        Assert.False(OpenAreas.OpensArea(ColdWaveStrikes));
    }

    [Fact]
    public void Every_stale_task_comes_from_an_event_that_opens_an_area()
    {
        foreach (var eventId in OpenAreas.StaleEvents(_ => true)) Assert.True(OpenAreas.OpensArea(eventId));
    }

    [Fact]
    public void A_run_with_everything_open_plans_no_change_however_often_it_is_checked()
    {
        var tags = new HashSet<string> { OpenAreas.SecondFloor, OpenAreas.ColdStorage, OpenAreas.RightHut, OpenAreas.LeftHut, OpenAreas.Garage };

        var first = OpenAreas.Plan(WarehouseManager, AfterDisaster, tags.Contains);

        Assert.True(first.IsEmpty);
        Assert.Same(first, OpenAreas.Plan(WarehouseManager, AfterDisaster, tags.Contains));
    }

    [Fact]
    public void A_running_repair_the_stairs_task_is_ended_once_the_second_floor_is_open()
    {
        var tags = new HashSet<string> { OpenAreas.SecondFloor };

        var stale = OpenAreas.StaleEvents(tags.Contains);

        Assert.Contains(RepairTheStairs, stale);
        Assert.Contains(FindSecondFloorKey, stale);
    }

    [Fact]
    public void Tasks_for_areas_still_locked_are_left_running()
    {
        var tags = new HashSet<string> { OpenAreas.SecondFloor };

        var stale = OpenAreas.StaleEvents(tags.Contains);

        Assert.DoesNotContain(ClearTheCellar, stale);
        Assert.DoesNotContain(GarageKey, stale);
        Assert.Empty(OpenAreas.StaleEvents(_ => false));
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
