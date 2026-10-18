using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class BatteryCapacityTests
{
    private const int Generator = 3;

    [Fact]
    public void A_battery_holds_its_capacity_times_the_multiplier()
    {
        Assert.Equal(25000f, BatteryCapacity.Scale(BatteryCapacity.BatteryType, 10000f, 2.5f));
    }

    [Fact]
    public void Other_electrical_devices_keep_their_own_numbers()
    {
        Assert.Equal(40f, BatteryCapacity.Scale(Generator, 40f, 2.5f));
    }
}
