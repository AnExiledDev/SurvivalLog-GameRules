using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

/// <summary>
/// The game builds its tables once, so a table writer looks at them until one pass has written them and then stops,
/// instead of reading them every second for the whole session.
/// </summary>
public sealed class TablePassTests
{
    [Fact]
    public void The_first_check_runs_a_pass()
    {
        var pass = new TablePass(retrySeconds: 2f);

        Assert.True(pass.IsDue(0f));
    }

    /// <summary>The plugin loads before the tables, so a pass that found nothing is tried again, but not every frame.</summary>
    [Fact]
    public void A_pass_that_found_no_table_is_tried_again_after_the_wait()
    {
        var pass = new TablePass(retrySeconds: 2f);

        pass.IsDue(0f);

        Assert.False(pass.IsDue(1f));
        Assert.True(pass.IsDue(2f));
    }

    [Fact]
    public void After_a_pass_is_done_nothing_runs_again()
    {
        var pass = new TablePass(retrySeconds: 2f);

        pass.IsDue(0f);
        pass.Done();

        Assert.False(pass.IsDue(2f));
        Assert.False(pass.IsDue(100f));
        Assert.False(pass.IsDue(10000f));
    }

    /// <summary>A setting change or a load runs one more pass at the next check, without waiting out the retry.</summary>
    [Fact]
    public void A_redo_runs_one_more_pass_at_the_next_check()
    {
        var pass = new TablePass(retrySeconds: 2f);

        pass.IsDue(0f);
        pass.Done();
        pass.Redo();

        Assert.True(pass.IsDue(0.5f));

        pass.Done();

        Assert.False(pass.IsDue(5f));
    }
}
