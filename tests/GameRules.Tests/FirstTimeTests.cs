using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class FirstTimeTests
{
    [Fact]
    public void Each_id_is_new_once_and_ids_do_not_share_a_turn()
    {
        var firstTime = new FirstTime();

        Assert.True(firstTime.Of(40019));
        Assert.False(firstTime.Of(40019));
        Assert.False(firstTime.Of(40019));
        Assert.True(firstTime.Of(10010));
        Assert.False(firstTime.Of(10010));
    }
}
