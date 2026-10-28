using System;
using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class OnceReadyTests
{
    [Fact]
    public void Waits_while_the_game_is_not_ready()
    {
        var runs = 0;
        var once = new OnceReady(() => false, () => runs++);

        once.Tick(0f);
        once.Tick(1f);

        Assert.Equal(0, runs);
    }

    /// <summary>The zone reads are warmed while the save loads, not again at every check of the session.</summary>
    [Fact]
    public void Runs_once_at_the_first_ready_check_and_never_again()
    {
        var isReady = false;
        var runs = 0;
        var once = new OnceReady(() => isReady, () => runs++);

        once.Tick(0f);
        isReady = true;
        once.Tick(1f);
        once.Tick(2f);
        once.Tick(3f);

        Assert.Equal(1, runs);
    }

    [Fact]
    public void A_job_that_throws_is_not_tried_again()
    {
        var runs = 0;
        var once = new OnceReady(() => true, () =>
        {
            runs++;
            throw new InvalidOperationException("no zone table yet");
        });

        Assert.Throws<InvalidOperationException>(() => once.Tick(0f));
        once.Tick(1f);

        Assert.Equal(1, runs);
    }
}
