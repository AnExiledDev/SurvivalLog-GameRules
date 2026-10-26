using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class TableKeysTests
{
    private readonly TableChanges _changes = new();
    private readonly TableKeeper _keeper = new();

    private TableKeys Keys() => new(_changes, _keeper);

    /// <summary>What the table check built every second before the list was kept: the keeper's keys, then the changes', each once.</summary>
    private List<string> BuiltFresh() => _keeper.Keys.Concat(_changes.ByKey().Keys).Distinct().ToList();

    [Fact]
    public void With_no_settings_changed_there_are_no_keys_to_check()
    {
        Assert.Empty(Keys().Keys);
        Assert.Empty(Keys().ByKey);
    }

    [Fact]
    public void Checking_again_with_no_setting_changed_builds_nothing_new()
    {
        _changes.Set("food", new[] { new SetTo("TiredOfFoodTime", 1f) });
        var keys = Keys();

        var first = keys.Keys;
        var firstByKey = keys.ByKey;

        Assert.Same(first, keys.Keys);
        Assert.Same(firstByKey, keys.ByKey);
    }

    [Fact]
    public void A_changed_setting_is_seen_at_the_next_check()
    {
        var keys = Keys();

        _changes.Set("food", new[] { new SetTo("TiredOfFoodTime", 1f) });
        var before = keys.Keys;

        _changes.Set("visitors", new[] { new SetTo("Trust_HungerLoss", 0f) });

        Assert.NotSame(before, keys.Keys);
        Assert.Equal(new[] { "TiredOfFoodTime", "Trust_HungerLoss" }, keys.Keys.OrderBy(key => key, StringComparer.Ordinal));
        Assert.True(keys.ByKey.ContainsKey("Trust_HungerLoss"));
    }

    [Fact]
    public void A_list_already_handed_out_is_not_changed_under_its_reader()
    {
        var keys = Keys();

        _changes.Set("food", new[] { new SetTo("TiredOfFoodTime", 1f) });
        var before = keys.Keys;

        _changes.Set("food", new[] { new SetTo("TiredOfFoodTime", 1f), new SetTo("FoodSpoil", 2f) });
        _ = keys.Keys;

        Assert.Equal(new[] { "TiredOfFoodTime" }, before);
    }

    [Fact]
    public void A_setting_turned_back_to_the_games_rule_is_still_checked_so_its_value_goes_back()
    {
        var keys = Keys();
        var change = new SetTo("StrayZombieEnabled", 0f);

        _changes.Set("zombies", new[] { change });
        Assert.Equal(new[] { "StrayZombieEnabled" }, keys.Keys);
        _keeper.Target("StrayZombieEnabled", 1, 1f, keys.ByKey["StrayZombieEnabled"]);

        _changes.Set("zombies", new TableChange[0]);

        Assert.Equal(new[] { "StrayZombieEnabled" }, keys.Keys);
        Assert.False(keys.ByKey.ContainsKey("StrayZombieEnabled"));
    }

    [Fact]
    public void The_kept_keys_and_changes_are_the_same_as_building_them_fresh_each_check()
    {
        var keys = Keys();

        _changes.Set("food", new[] { new SetTo("TiredOfFoodTime", 1f), new SetTo("FoodSpoil", 2f) });
        Assert.Equal(BuiltFresh(), keys.Keys);
        foreach (var key in keys.Keys) _keeper.Target(key, 1, 5f, keys.ByKey.TryGetValue(key, out var change) ? change : null);

        _changes.Set("visitors", new[] { new SetTo("Trust_HungerLoss", 0f) });
        Assert.Equal(BuiltFresh(), keys.Keys);

        _changes.Set("food", new[] { new SetTo("FoodSpoil", 3f) });
        Assert.Equal(BuiltFresh(), keys.Keys);
        Assert.Equal(_changes.ByKey(), keys.ByKey);
    }

    [Fact]
    public void Each_set_moves_the_version_even_to_the_same_changes()
    {
        var before = _changes.Version;

        _changes.Set("food", new TableChange[0]);
        _changes.Set("food", new TableChange[0]);

        Assert.Equal(before + 2, _changes.Version);
    }
}
