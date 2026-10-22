using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class TableKeeperTests
{
    [Fact]
    public void A_key_the_mod_never_changed_is_left_alone()
    {
        var keeper = new TableKeeper();

        Assert.Null(keeper.Target("RatVisit_BaseChance", entryId: 1, entryValue: 0.05f, change: null));
    }

    [Fact]
    public void A_scale_multiplies_the_games_value_and_does_not_compound_on_its_own_writes()
    {
        var keeper = new TableKeeper();
        var change = new ScaleBy("ThinkCD", 2f);

        Assert.Equal(7200f, keeper.Target("ThinkCD", 1, 3600f, change));
        Assert.Equal(7200f, keeper.Target("ThinkCD", 1, 7200f, change));
    }

    [Fact]
    public void Turning_a_setting_back_to_the_games_rule_restores_the_games_value()
    {
        var keeper = new TableKeeper();

        keeper.Target("StrayZombieEnabled", 1, 1f, new SetTo("StrayZombieEnabled", 0f));

        Assert.Equal(1f, keeper.Target("StrayZombieEnabled", 1, 0f, change: null));
    }

    [Fact]
    public void A_reloaded_table_entry_holds_the_games_value_again_and_is_reread()
    {
        var keeper = new TableKeeper();
        var change = new ScaleBy("ThinkCD", 2f);

        keeper.Target("ThinkCD", 1, 3600f, change);

        Assert.Equal(8000f, keeper.Target("ThinkCD", entryId: 2, entryValue: 4000f, change));
    }

    [Fact]
    public void Each_group_replaces_only_its_own_changes()
    {
        var changes = new TableChanges();

        changes.Set("food", new[] { new SetTo("TiredOfFoodTime", 1f) });
        changes.Set("visitors", new[] { new SetTo("Trust_HungerLoss", 0f) });
        changes.Set("food", new TableChange[0]);

        var byKey = changes.ByKey();

        Assert.False(byKey.ContainsKey("TiredOfFoodTime"));
        Assert.True(byKey.ContainsKey("Trust_HungerLoss"));
    }
}
