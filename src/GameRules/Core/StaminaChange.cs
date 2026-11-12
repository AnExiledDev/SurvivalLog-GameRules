namespace GameRules.Core;

/// <summary>
/// Picks the multiplier for one change to the player's stamina, from what AttributeComponent.CostAttrValue is told
/// about it. Action costs come with the None or Action argument and a None, Furniture or Action source; stamina lost
/// over time (the Natural argument) and to buffs is left alone. Stimulant gains come from an item use (the source is
/// Item and the id is the item's) or from a stimulant buff ticking (the source is Buff and the id is the buff's).
/// </summary>
public static class StaminaChange
{
    public const int ArgsNone = 0;
    public const int ArgsAction = 1;

    public const int SourceNone = 0;
    public const int SourceBuff = 1;
    public const int SourceItem = 2;
    public const int SourceFurniture = 3;
    public const int SourceAction = 5;

    public static float Factor(float value, int argsType, int sourceType, int configId, float actionCost, float stimulantStrength)
    {
        if (value < 0f) return IsActionCost(argsType, sourceType) ? actionCost : 1f;

        if (value > 0f && IsStimulant(sourceType, configId)) return stimulantStrength;

        return 1f;
    }

    private static bool IsActionCost(int argsType, int sourceType) =>
        (argsType == ArgsNone || argsType == ArgsAction)
        && (sourceType == SourceNone || sourceType == SourceFurniture || sourceType == SourceAction);

    private static bool IsStimulant(int sourceType, int configId) =>
        (sourceType == SourceItem && Stimulants.Items.Contains(configId))
        || (sourceType == SourceBuff && Stimulants.Buffs.Contains(configId));
}
