using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRules.Core;

/// <summary>
/// The GlobalSetting changes every settings group wants right now. Each group replaces its own list whenever its
/// settings change, so a group never has to know what the others asked for.
/// </summary>
public sealed class TableChanges
{
    private readonly Dictionary<string, IReadOnlyList<TableChange>> _byGroup = new();

    public void Set(string group, IEnumerable<TableChange> changes) => _byGroup[group] = changes.ToList();

    /// <summary>One change per key. Two groups never share a key; if they did, the later group in name order wins.</summary>
    public IReadOnlyDictionary<string, TableChange> ByKey()
    {
        var byKey = new Dictionary<string, TableChange>();

        foreach (var group in _byGroup.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            foreach (var change in group.Value) byKey[change.Key] = change;
        }

        return byKey;
    }
}
