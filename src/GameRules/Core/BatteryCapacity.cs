namespace GameRules.Core;

public static class BatteryCapacity
{
    /// <summary>The game's electrical type for a battery, the only type it counts toward stored power.</summary>
    public const int BatteryType = 5;

    /// <summary>A battery's capacity times the player's multiplier. Every other electrical device keeps its own number.</summary>
    public static float Scale(int electricalType, float gameCapacity, float multiplier) =>
        electricalType == BatteryType ? gameCapacity * multiplier : gameCapacity;
}
