namespace GameRules.Core;

/// <summary>
/// An int GlobalSetting lives in Params1, but some game code reads the same key's Params2 (music and perfect-harvest
/// morale do). Where the table keeps both columns equal, a write keeps them equal; anywhere else Params2 means
/// something of its own and is left alone.
/// </summary>
public static class IntEntry
{
    public static bool KeepsParams2InStep(int params1, float params2) => params2 == params1;
}
