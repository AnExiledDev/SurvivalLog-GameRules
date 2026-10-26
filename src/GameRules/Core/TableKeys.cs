using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>
/// The GlobalSetting keys to check each second, and the change wanted for each: every key a group changes now and
/// every key the mod ever changed (so a dropped one gets the game's value back). Rebuilt only when a group's changes
/// are set again, since the table is checked every second and the changes move only when a setting does. The keeper
/// only learns a key while a change asks for it, and that key is already here, so nothing it learns is missed.
/// </summary>
public sealed class TableKeys
{
    private static readonly IReadOnlyDictionary<string, TableChange> NoChanges = new Dictionary<string, TableChange>();

    private readonly TableChanges _changes;
    private readonly TableKeeper _keeper;
    private List<string> _keys = new();
    private IReadOnlyDictionary<string, TableChange> _byKey = NoChanges;
    private int _builtVersion = -1;

    public TableKeys(TableChanges changes, TableKeeper keeper)
    {
        _changes = changes;
        _keeper = keeper;
    }

    /// <summary>Every key to check: the keeper's first, then the changes', each once.</summary>
    public IReadOnlyList<string> Keys
    {
        get
        {
            Refresh();

            return _keys;
        }
    }

    /// <summary>One change per key, as <see cref="TableChanges.ByKey"/> gives it.</summary>
    public IReadOnlyDictionary<string, TableChange> ByKey
    {
        get
        {
            Refresh();

            return _byKey;
        }
    }

    private void Refresh()
    {
        if (_changes.Version == _builtVersion) return;

        _builtVersion = _changes.Version;
        _byKey = _changes.ByKey();

        var seen = new HashSet<string>();
        var keys = new List<string>();

        foreach (var key in _keeper.Keys)
        {
            if (seen.Add(key)) keys.Add(key);
        }

        foreach (var key in _byKey.Keys)
        {
            if (seen.Add(key)) keys.Add(key);
        }

        _keys = keys;
    }
}
