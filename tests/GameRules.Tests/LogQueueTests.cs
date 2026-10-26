using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

/// <summary>The worker is played by hand here: a scheduled write runs only when the test runs it.</summary>
public sealed class LogQueueTests
{
    private static readonly DateTime At = new(2026, 10, 26, 13, 5, 7, 89);

    private readonly List<Action> _scheduled = new();
    private readonly List<string> _written = new();
    private Action<string>? _onAppend;

    private LogQueue Queue() => new(text =>
    {
        _onAppend?.Invoke(text);
        _written.Add(text);
    }, work => _scheduled.Add(work));

    private static string Line(string message) => "2026-10-26 13:05:07.089 " + message + Environment.NewLine;

    [Fact]
    public void A_line_is_written_with_its_time_on_the_worker_not_when_it_is_added()
    {
        var queue = Queue();

        queue.Add(At, "pool on");

        Assert.Empty(_written);

        _scheduled.Single()();

        Assert.Equal(new[] { Line("pool on") }, _written);
    }

    [Fact]
    public void A_burst_of_lines_is_one_write_in_the_order_they_came()
    {
        var queue = Queue();

        queue.Add(At, "one");
        queue.Add(At, "two");
        queue.Add(At, "three");

        Assert.Single(_scheduled);

        _scheduled[0]();

        Assert.Equal(new[] { Line("one") + Line("two") + Line("three") }, _written);
    }

    [Fact]
    public void A_line_added_after_a_write_finished_gets_a_write_of_its_own()
    {
        var queue = Queue();

        queue.Add(At, "one");
        _scheduled[0]();
        queue.Add(At, "two");

        Assert.Equal(2, _scheduled.Count);

        _scheduled[1]();

        Assert.Equal(new[] { Line("one"), Line("two") }, _written);
    }

    [Fact]
    public void A_line_added_while_a_write_runs_is_written_by_that_same_worker()
    {
        var queue = Queue();
        var isFirst = true;

        _onAppend = _ =>
        {
            if (!isFirst) return;

            isFirst = false;
            queue.Add(At, "two");
        };

        queue.Add(At, "one");
        _scheduled[0]();

        Assert.Single(_scheduled);
        Assert.Equal(new[] { Line("one"), Line("two") }, _written);
    }

    [Fact]
    public void A_failed_write_drops_its_lines_and_never_stops_later_ones()
    {
        var queue = Queue();

        _onAppend = _ => throw new IOException("file is locked");
        queue.Add(At, "lost");
        _scheduled[0]();

        _onAppend = null;
        queue.Add(At, "kept");

        Assert.Equal(2, _scheduled.Count);

        _scheduled[1]();

        Assert.Equal(new[] { Line("kept") }, _written);
    }

    [Fact]
    public void When_no_worker_can_be_had_the_line_waits_for_the_next_one()
    {
        var refuses = true;
        var queue = new LogQueue(text => _written.Add(text), work =>
        {
            if (refuses) throw new InvalidOperationException("no threads");

            _scheduled.Add(work);
        });

        queue.Add(At, "one");
        refuses = false;
        queue.Add(At, "two");

        _scheduled.Single()();

        Assert.Equal(new[] { Line("one") + Line("two") }, _written);
    }
}
