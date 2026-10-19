using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRules.Core;

/// <summary>The tags to add and remove so every area the role opens with a key is open.</summary>
public sealed record AreaChange(IReadOnlyList<string> Add, IReadOnlyList<string> Remove)
{
    public bool IsEmpty => Add.Count == 0 && Remove.Count == 0;
}

/// <summary>
/// Starts a run with every locked area open. The game opens an area by adding a tag (the Wage Slave's and Student's
/// second floor and basement, the Warehouse Manager's second floor, cold storage, garage and two side rooms), and
/// reads that tag for navigation, building and the floor menu, so adding the tags opens the areas. The story events
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

    /// <summary>What to change for this role. Only missing tags are added and only present blockers removed, so an open run changes nothing.</summary>
    public static AreaChange Plan(int role, Func<string, bool> hasTag)
    {
        var (areas, blockers) = role switch
        {
            WageSlave or Student => (HouseAreas, HouseBlockers),
            WarehouseManager => (WarehouseAreas, Array.Empty<string>()),
            _ => (Array.Empty<string>(), Array.Empty<string>()),
        };

        return new AreaChange(areas.Where(tag => !hasTag(tag)).ToList(), blockers.Where(hasTag).ToList());
    }

    /// <summary>An event that opens an area is skipped once that area is already open.</summary>
    public static bool SkipsEvent(int eventId, Func<string, bool> hasTag) =>
        OpeningEvents.TryGetValue(eventId, out var area) && hasTag(area);

    /// <summary>
    /// Whether a spot on a key-locked floor still counts as locked for building before the disaster. The game greys out
    /// every such spot until the disaster; one whose floor tag is already there is open.
    /// </summary>
    public static bool IsFloorStillLocked(string? floorTag, Func<string, bool> hasTag) =>
        !string.IsNullOrEmpty(floorTag) && !hasTag(floorTag);
}
