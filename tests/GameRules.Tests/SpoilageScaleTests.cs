using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class SpoilageScaleTests
{
    [Fact]
    public void The_speed_multiplies_a_rooms_decay_rate()
    {
        Assert.Equal(0.5f, SpoilageScale.Apply(1f, 0.5f));
        Assert.Equal(1.6f, SpoilageScale.Apply(0.8f, 2f), precision: 5);
    }

    [Fact]
    public void A_room_where_food_does_not_spoil_stays_that_way()
    {
        Assert.Equal(0f, SpoilageScale.Apply(0f, 3f));
    }

    [Fact]
    public void A_slow_speed_never_drops_below_the_slowest_rate_the_game_accepts()
    {
        Assert.Equal(SpoilageScale.MinGameScale, SpoilageScale.Apply(0.01f, 0.1f));
    }
}
