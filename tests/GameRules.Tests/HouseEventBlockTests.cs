using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class HouseEventBlockTests
{
    private const int PowerCutWageSlave = 40019;
    private const int HeavyRainShared = 10010;
    private const int HeavyRainWageSlave = 40700;
    private const int HeavyRainStudent = 52700;
    private const int HeavyRainWarehouseManager = 62700;
    private const int LeakInTheEaves = 40826;

    [Fact]
    public void The_games_rules_hold_back_nothing()
    {
        Assert.False(HouseEventBlock.Game.BlocksAny);
        Assert.False(HouseEventBlock.Game.BlocksEvent(HeavyRainWageSlave));
    }

    [Fact]
    public void No_storm_leaks_stops_every_heavy_rain_event_but_leaves_the_power_cut_and_the_eaves()
    {
        var block = HouseEventBlock.Game with { NoStormLeaks = true };

        Assert.True(block.BlocksAny);
        Assert.True(block.BlocksEvent(HeavyRainShared));
        Assert.True(block.BlocksEvent(HeavyRainWageSlave));
        Assert.True(block.BlocksEvent(HeavyRainStudent));
        Assert.True(block.BlocksEvent(HeavyRainWarehouseManager));
        Assert.False(block.BlocksEvent(LeakInTheEaves));
        Assert.False(block.BlocksEvent(PowerCutWageSlave));
    }
}
