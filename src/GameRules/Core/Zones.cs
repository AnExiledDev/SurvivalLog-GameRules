namespace GameRules.Core;

/// <summary>
/// The game's climate zones by the area id it gives them (EnvAreaHelper). A position outside every zone box counts as
/// Indoor. Once the greenhouse is built, the terrace's greenhouse box already counts as the Sunroom.
/// </summary>
public static class Zones
{
    public const int Indoor = 1;
    public const int Sunroom = 2;
    public const int Terrace = 3;
    public const int ColdStorage = 4;
    public const int Basement = 5;

    public static string Name(int zone) => zone switch
    {
        Indoor => "Indoor",
        Sunroom => "Sunroom",
        Terrace => "Terrace",
        ColdStorage => "Cold storage",
        Basement => "Basement",
        _ => $"zone {zone}",
    };
}
