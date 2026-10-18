using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class RestartKeeperTests
{
    [Fact]
    public void A_fresh_run_keeps_the_games_cap()
    {
        var keeper = new RestartKeeper();

        Assert.Null(keeper.Next(current: 3, used: 0));
    }

    [Fact]
    public void Each_restart_used_raises_the_cap_so_the_full_allowance_stays()
    {
        var keeper = new RestartKeeper();

        var write = keeper.Next(current: 3, used: 2);

        Assert.Equal(new RestartWrite(Max: 5, GameMax: 3, IsPutBack: false), write);
    }

    [Fact]
    public void Running_past_the_games_own_cap_still_leaves_the_full_allowance()
    {
        var keeper = new RestartKeeper();

        keeper.Next(current: 3, used: 2);

        Assert.Equal(10, keeper.Next(current: 5, used: 7)?.Max);
    }

    [Fact]
    public void A_cap_the_mod_wrote_is_never_taken_as_the_games_own()
    {
        var keeper = new RestartKeeper();

        keeper.Next(current: 3, used: 2);

        Assert.Null(keeper.Next(current: 5, used: 2));
        Assert.Equal(4, keeper.Next(current: 5, used: 1)?.Max);
    }

    [Fact]
    public void Loading_a_run_with_fewer_restarts_used_lowers_the_cap_back()
    {
        var keeper = new RestartKeeper();

        keeper.Next(current: 3, used: 4);

        Assert.Equal(3, keeper.Next(current: 7, used: 0)?.Max);
    }

    [Fact]
    public void A_cap_changed_by_something_else_is_put_back()
    {
        var keeper = new RestartKeeper();

        keeper.Next(current: 3, used: 2);

        Assert.Equal(new RestartWrite(Max: 5, GameMax: 3, IsPutBack: true), keeper.Next(current: 1, used: 2));
    }

    [Fact]
    public void Nothing_is_written_before_the_game_loads_its_cap_or_a_run()
    {
        var keeper = new RestartKeeper();

        Assert.Null(keeper.Next(current: 0, used: 2));
        Assert.Null(keeper.Next(current: 3, used: null));
        Assert.Equal(5, keeper.Next(current: 3, used: 2)?.Max);
    }
}
