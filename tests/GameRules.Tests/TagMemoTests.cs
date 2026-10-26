using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

/// <summary>Each question the memo saves hands the game a new copy of the tag's text, so the count of questions is the cost under test.</summary>
public sealed class TagMemoTests
{
    private readonly HashSet<string> _tags = new() { OpenAreas.SecondFloor };
    private readonly List<string> _asked = new();

    private bool Read(string tag)
    {
        _asked.Add(tag);

        return _tags.Contains(tag);
    }

    [Fact]
    public void Each_tag_is_asked_once_per_check_and_answers_as_the_game_does()
    {
        var memo = new TagMemo();

        memo.Use(Read);

        for (var i = 0; i < 5; i++)
        {
            Assert.True(memo.HasTag(OpenAreas.SecondFloor));
            Assert.False(memo.HasTag(OpenAreas.Basement));
        }

        Assert.Equal(new[] { OpenAreas.SecondFloor, OpenAreas.Basement }, _asked);
    }

    [Fact]
    public void After_the_mod_changes_the_tags_they_are_asked_again()
    {
        var memo = new TagMemo();

        memo.Use(Read);
        memo.HasTag(OpenAreas.Basement);

        _tags.Add(OpenAreas.Basement);
        memo.Forget();

        Assert.True(memo.HasTag(OpenAreas.Basement));
    }

    [Fact]
    public void A_new_check_asks_again_so_a_tag_the_game_changed_in_between_is_seen()
    {
        var memo = new TagMemo();

        memo.Use(Read);
        Assert.False(memo.HasTag(OpenAreas.Basement));
        memo.Use(null);

        _tags.Add(OpenAreas.Basement);
        memo.Use(Read);

        Assert.True(memo.HasTag(OpenAreas.Basement));
    }

    [Fact]
    public void The_same_reader_is_handed_out_every_time()
    {
        var memo = new TagMemo();

        Assert.Same(memo.HasTag, memo.HasTag);
    }

    [Fact]
    public void Asking_outside_a_check_is_an_error_rather_than_an_old_answer()
    {
        var memo = new TagMemo();

        memo.Use(Read);
        memo.HasTag(OpenAreas.SecondFloor);
        memo.Use(null);

        Assert.Throws<InvalidOperationException>(() => memo.HasTag(OpenAreas.SecondFloor));
    }

    [Fact]
    public void A_plan_through_the_memo_is_the_same_as_asking_the_game_directly()
    {
        var memo = new TagMemo();
        var tags = new HashSet<string> { "TAG_STAIRS_BROKEN", OpenAreas.Basement };

        memo.Use(tags.Contains);

        foreach (var role in new[] { 1, 2, 3, 4 })
        {
            var direct = OpenAreas.Plan(role, isBeforeDisaster: false, tags.Contains);
            var memoised = OpenAreas.Plan(role, isBeforeDisaster: false, memo.HasTag);

            Assert.Equal(direct.Add, memoised.Add);
            Assert.Equal(direct.Remove, memoised.Remove);
        }

        Assert.Equal(OpenAreas.StaleEvents(tags.Contains), OpenAreas.StaleEvents(memo.HasTag));
    }
}
