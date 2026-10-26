using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class PoolWarmUpTests
{
    /// <summary>
    /// The warm-up's house: each pool zone (Indoor 1, Sunroom 2, Terrace 3, Basement 5) holds its own id in device
    /// heat, the Terrace takes no device heat, and the AC is in the Sunroom. Every zone's pooled device heat is the
    /// whole pool, 11, four times; only the Terrace gets warmth on top, 11; and the three zones other than the Sunroom
    /// share its AC. 44 + 11 + 3.
    /// </summary>
    private const int EveryAnswer = 58;

    [Fact]
    public void The_warm_up_runs_every_pooled_answer_the_hooks_give()
    {
        Assert.Equal(EveryAnswer, PoolWarmUp.Run());
    }

    [Fact]
    public void Running_it_again_changes_nothing()
    {
        PoolWarmUp.Run();

        Assert.Equal(EveryAnswer, PoolWarmUp.Run());
    }
}
