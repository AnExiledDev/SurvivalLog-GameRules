using System.Collections.Generic;

namespace GameRules.Core;

/// <summary>The items and buffs that give stamina, from the game's item, action and buff tables (game 1.1.20719).</summary>
public static class Stimulants
{
    /// <summary>
    /// Coffee (7 and 2526), Sports Drink, Instant Coffee, Adrenaline, Stimulants, the three Coffee and three Honey
    /// Coffee qualities, and the two coffees she makes.
    /// </summary>
    public static IReadOnlySet<int> Items { get; } = new HashSet<int>
    {
        7, 2131, 2136, 2526, 2403, 2404, 12425, 12426, 12427, 12449, 12450, 12451, 52285, 52425,
    };

    /// <summary>The Sports Drink buff (5 stamina an hour for 12 hours) and the Stimulants buff (5 an hour for 24 hours).</summary>
    public static IReadOnlySet<int> Buffs { get; } = new HashSet<int> { 20020, 24040 };
}
