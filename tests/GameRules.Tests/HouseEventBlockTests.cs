using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class HouseEventBlockTests
{
    private const int PowerCutWageSlave = 40019;
    private const int PowerCutStudent = 52019;
    private const int PowerCutWarehouseManager = 62019;
    private const int HeavyRainShared = 10010;
    private const int HeavyRainWageSlave = 40700;
    private const int HeavyRainStudent = 52700;
    private const int HeavyRainWarehouseManager = 62700;
    private const int LeakInTheEaves = 40826;
    private const int CutDay = HouseEventBlock.GamePowerCutDay;

    [Fact]
    public void The_games_rules_hold_back_nothing()
    {
        Assert.False(HouseEventBlock.Game.BlocksAny);
        Assert.False(HouseEventBlock.Game.BlocksEvent(PowerCutWageSlave, CutDay));
        Assert.False(HouseEventBlock.Game.BlocksEvent(HeavyRainWageSlave, CutDay));
    }

    [Fact]
    public void A_delay_holds_the_power_cut_back_until_its_last_day_and_no_longer()
    {
        var block = HouseEventBlock.Game with { PowerCutDelay = 10 };

        Assert.True(block.BlocksAny);
        Assert.True(block.BlocksEvent(PowerCutWageSlave, CutDay));
        Assert.True(block.BlocksEvent(PowerCutWageSlave, CutDay + 9));
        Assert.False(block.BlocksEvent(PowerCutWageSlave, CutDay + 10));
    }

    [Fact]
    public void A_delay_covers_every_characters_power_cut()
    {
        var block = HouseEventBlock.Game with { PowerCutDelay = 5 };

        Assert.True(block.BlocksEvent(PowerCutStudent, CutDay));
        Assert.True(block.BlocksEvent(PowerCutWarehouseManager, CutDay));
    }

    [Fact]
    public void A_delayed_power_cut_goes_ahead_when_the_day_cannot_be_read()
    {
        var block = HouseEventBlock.Game with { PowerCutDelay = 10 };

        Assert.False(block.BlocksEvent(PowerCutWageSlave, null));
    }

    [Fact]
    public void No_power_cut_holds_every_characters_cut_back_on_any_day()
    {
        var block = HouseEventBlock.Game with { NoPowerCut = true };

        Assert.True(block.BlocksEvent(PowerCutWageSlave, 999));
        Assert.True(block.BlocksEvent(PowerCutStudent, null));
        Assert.True(block.BlocksEvent(PowerCutWarehouseManager, CutDay));
        Assert.False(block.BlocksEvent(HeavyRainWageSlave, CutDay));
    }

    /// <summary>The event hook reads the game day only for an event that needs it; for every other event the day must not matter.</summary>
    [Theory]
    [InlineData(HeavyRainShared)]
    [InlineData(HeavyRainWageSlave)]
    [InlineData(HeavyRainStudent)]
    [InlineData(HeavyRainWarehouseManager)]
    [InlineData(LeakInTheEaves)]
    [InlineData(0)]
    public void An_event_that_is_not_the_power_cut_is_answered_the_same_without_the_day(int eventId)
    {
        Assert.False(HouseEventBlock.ReadsDay(eventId));

        foreach (var block in EveryBlock())
        {
            foreach (var day in new[] { 0, CutDay - 1, CutDay, CutDay + 9, CutDay + 10, 999 })
            {
                Assert.Equal(block.BlocksEvent(eventId, day), block.BlocksEvent(eventId, null));
            }
        }
    }

    [Theory]
    [InlineData(PowerCutWageSlave)]
    [InlineData(PowerCutStudent)]
    [InlineData(PowerCutWarehouseManager)]
    public void The_power_cut_is_answered_with_the_day(int eventId)
    {
        Assert.True(HouseEventBlock.ReadsDay(eventId));
    }

    private static IEnumerable<HouseEventBlock> EveryBlock()
    {
        foreach (var delay in new[] { 0, 1, 10 })
        foreach (var noPowerCut in new[] { false, true })
        foreach (var noStormLeaks in new[] { false, true })
            yield return new HouseEventBlock(delay, noPowerCut, noStormLeaks);
    }

    [Fact]
    public void No_storm_leaks_stops_every_heavy_rain_event_but_leaves_the_power_cut_and_the_eaves()
    {
        var block = HouseEventBlock.Game with { NoStormLeaks = true };

        Assert.True(block.BlocksEvent(HeavyRainShared, 3));
        Assert.True(block.BlocksEvent(HeavyRainWageSlave, 3));
        Assert.True(block.BlocksEvent(HeavyRainStudent, 3));
        Assert.True(block.BlocksEvent(HeavyRainWarehouseManager, 3));
        Assert.False(block.BlocksEvent(LeakInTheEaves, 3));
        Assert.False(block.BlocksEvent(PowerCutWageSlave, CutDay));
    }
}
