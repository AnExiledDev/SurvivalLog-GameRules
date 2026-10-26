using System;
using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>The tags to add and remove so every area the role opens with a key is open.</summary>
public sealed record AreaChange(IReadOnlyList<string> Add, IReadOnlyList<string> Remove)
{
    public bool IsEmpty => Add.Count == 0 && Remove.Count == 0;
}

/// <summary>
/// Opens every locked area once the disaster has happened. The game opens an area by adding a tag (the Wage Slave's and
/// Student's second floor and basement, the Warehouse Manager's second floor, cold storage, garage and two side rooms),
/// and reads that tag for navigation, building and the floor menu, so adding the tags opens the areas. Before the
/// disaster the house is a separate map with no way onto the other floors, so nothing is opened there. The story events
/// that would open an area again (finding the key, clearing the stairs) are skipped once its tag is there.
/// </summary>
public static class OpenAreas
{
    public const string SecondFloor = "TAG_FLOOR_2F_UNLOCKED";
    public const string Basement = "TAG_FLOOR_B1_UNLOCKED";
    public const string ColdStorage = "WH_ColdstoreOpen";
    public const string RightHut = "WH_HutROpen";
    public const string LeftHut = "WH_HutLOpen";
    public const string Garage = "WH_GarageOpen";

    private const int WageSlave = 1;
    private const int Student = 2;
    private const int WarehouseManager = 3;

    private static readonly string[] HouseAreas = { SecondFloor, Basement };

    /// <summary>The broken stairs and the blocked cellar, which the house roles' unlock events clear.</summary>
    private static readonly string[] HouseBlockers = { "TAG_STAIRS_BROKEN", "TAG_CELLAR_BLOCKED" };

    private static readonly string[] WarehouseAreas = { SecondFloor, ColdStorage, RightHut, LeftHut, Garage };

    private static readonly AreaChange Nothing = new(Array.Empty<string>(), Array.Empty<string>());

    /// <summary>Each area's tag and the events that open it: the key or repair story and its forced follow-ups.</summary>
    private static readonly Dictionary<int, string> OpeningEvents = new()
    {
        [30028] = SecondFloor, [40413] = SecondFloor, [40416] = SecondFloor, [51502] = SecondFloor, [51507] = SecondFloor,
        [40420] = Basement, [40417] = Basement, [51503] = Basement, [51512] = Basement,
        [61201] = ColdStorage, [61202] = ColdStorage,
        [61203] = RightHut, [61205] = RightHut,
        [61212] = LeftHut, [61204] = LeftHut,
        [61210] = Garage, [61211] = Garage,
    };

    /// <summary>
    /// What to change for this role. Nothing before the disaster; after it only missing tags are added and only present
    /// blockers removed, so an open run changes nothing.
    /// </summary>
    /// <remarks>Checked every second, so an open run (the usual answer) makes no new objects: plain loops, and one shared empty change.</remarks>
    public static AreaChange Plan(int role, bool isBeforeDisaster, Func<string, bool> hasTag)
    {
        if (isBeforeDisaster) return Nothing;

        var (areas, blockers) = role switch
        {
            WageSlave or Student => (HouseAreas, HouseBlockers),
            WarehouseManager => (WarehouseAreas, Array.Empty<string>()),
            _ => (Array.Empty<string>(), Array.Empty<string>()),
        };

        List<string>? add = null;
        List<string>? remove = null;

        foreach (var tag in areas)
        {
            if (!hasTag(tag)) (add ??= new List<string>()).Add(tag);
        }

        foreach (var tag in blockers)
        {
            if (hasTag(tag)) (remove ??= new List<string>()).Add(tag);
        }

        if (add == null && remove == null) return Nothing;

        return new AreaChange(add ?? (IReadOnlyList<string>)Array.Empty<string>(), remove ?? (IReadOnlyList<string>)Array.Empty<string>());
    }

    /// <summary>Whether the event is one that opens an area; only those need the save's tags.</summary>
    public static bool OpensArea(int eventId) => OpeningEvents.ContainsKey(eventId);

    /// <summary>An event that opens an area is skipped once that area is already open.</summary>
    public static bool SkipsEvent(int eventId, Func<string, bool> hasTag) =>
        OpeningEvents.TryGetValue(eventId, out var area) && hasTag(area);

    /// <summary>
    /// The opening events whose area is already open. One that was running before the area opened (a loaded save, or
    /// the event started before the disaster) is still on the task list, so these are ended.
    /// </summary>
    public static IReadOnlyList<int> StaleEvents(Func<string, bool> hasTag)
    {
        List<int>? stale = null;

        foreach (var entry in OpeningEvents)
        {
            if (hasTag(entry.Value)) (stale ??= new List<int>()).Add(entry.Key);
        }

        return stale ?? (IReadOnlyList<int>)Array.Empty<int>();
    }
}
