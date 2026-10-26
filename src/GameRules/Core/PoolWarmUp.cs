namespace GameRules.Core;

/// <summary>
/// Runs every pooled answer once on throwaway objects, so the runtime compiles the pool's code before the game's first
/// temperature read instead of during it. Pure .NET and touches nothing shared, so it can run on any thread.
/// </summary>
public static class PoolWarmUp
{
    /// <returns>The sum of every answer, for the test that it went all the way through.</returns>
    public static int Run()
    {
        var pool = new HeatPool(new PoolSettings(IndoorAndSunroom: true, Terrace: true, Basement: true));
        var climate = new PooledClimate(pool, new FrameClimate(new SteadyHouse(), () => 0));
        var total = 0;

        foreach (var zone in pool.Members)
        {
            total += climate.DeviceHeat(zone, zone);
            total += climate.Temperature(zone, 0);

            if (climate.IsAcPowered(zone, gameSaysPowered: false)) total++;
        }

        return total;
    }

    /// <summary>Each zone holds its own id in device heat, the Terrace takes no device heat, and the AC is in the Sunroom.</summary>
    private sealed class SteadyHouse : IGameClimate
    {
        public int DeviceHeatIn(int zone) => zone;

        public bool? TakesDeviceHeat(int zone) => zone != Zones.Terrace;

        public bool IsAcPoweredIn(int zone) => zone == Zones.Sunroom;

        public LoadedGame CurrentGame() => default;
    }
}
