using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRules.Core;

/// <summary>
/// Which zones share device heat, and how much each receives. The game warms a zone with the heat of the running
/// devices standing in it (its SumDeviceHeat), and only for zones its config lets take device heat at all; the
/// Terrace and Cold storage take none. In the pool every member receives the heat of the devices in every member,
/// each device counted once, so the sum is the same whichever member a device stands in. Indoor anchors the pool; it
/// only exists once a second zone joins. Zones outside the pool keep exactly the game's numbers.
/// </summary>
public sealed class HeatPool
{
    private readonly int[] _members;

    public HeatPool(PoolSettings settings)
    {
        var joined = new List<int> { Zones.Indoor };

        if (settings.IndoorAndSunroom) joined.Add(Zones.Sunroom);
        if (settings.Terrace) joined.Add(Zones.Terrace);
        if (settings.Basement) joined.Add(Zones.Basement);

        _members = joined.Count > 1 ? joined.ToArray() : Array.Empty<int>();
    }

    public IReadOnlyList<int> Members => _members;

    /// <summary>False when nothing joined Indoor: every zone keeps the game's heat and nothing needs hooking.</summary>
    public bool IsActive => _members.Length > 0;

    public bool Contains(int zone) => Array.IndexOf(_members, zone) >= 0;

    /// <summary>
    /// The device heat a zone the game warms with devices should count. <paramref name="heatInZone"/> is the game's
    /// own sum for that zone; <paramref name="heatIn"/> reads the game's sum for any other zone. Outside the pool the
    /// game's sum stands.
    /// </summary>
    public int DeviceHeatFor(int zone, int heatInZone, Func<int, int> heatIn)
    {
        if (!Contains(zone)) return heatInZone;

        var total = heatInZone;

        foreach (var member in _members)
        {
            if (member != zone) total += heatIn(member);
        }

        return total;
    }

    /// <summary>
    /// Warmth to add on top of the game's temperature for a zone. A zone the game already warms with devices gets its
    /// pooled heat through <see cref="DeviceHeatFor"/>, so it gets nothing here; a pool member the game gives no device
    /// heat (the Terrace) gets the whole pool's device heat, the devices standing in it included.
    /// </summary>
    public int ExtraWarmth(int zone, bool gameWarmsWithDevices, Func<int, int> heatIn)
    {
        if (gameWarmsWithDevices || !Contains(zone)) return 0;

        // A plain loop: this runs dozens of times a frame, and LINQ over an array makes a new enumerator each call.
        var total = 0;

        foreach (var member in _members) total = checked(total + heatIn(member));

        return total;
    }

    /// <summary>
    /// Whether a zone counts as having a powered AC. The game says yes only for an AC running in the zone itself; in the
    /// pool an AC running in any member counts for every member, so the game's AC checks agree with the pooled
    /// temperature. A yes from the game always stands, and zones outside the pool keep the game's answer.
    /// </summary>
    public bool IsAcPoweredFor(int zone, bool gameSaysPowered, Func<int, bool> poweredIn)
    {
        if (gameSaysPowered || !Contains(zone)) return gameSaysPowered;

        foreach (var member in _members)
        {
            if (member != zone && poweredIn(member)) return true;
        }

        return false;
    }

    public string Describe() =>
        IsActive ? string.Join(", ", _members.Select(Zones.Name)) : "nothing";
}
