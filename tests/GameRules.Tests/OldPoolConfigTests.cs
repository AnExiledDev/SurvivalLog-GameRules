using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class OldPoolConfigTests
{
    private const string OldConfig =
        "## Settings file was created by plugin Whole Floor Climate v0.1.1\r\n" +
        "\r\n" +
        "[Other]\r\n" +
        "Terrace = false\r\n" +
        "\r\n" +
        "[Pool]\r\n" +
        "\r\n" +
        "## Running AC units, heaters and stoves = warm both\r\n" +
        "# Setting type: Boolean\r\n" +
        "# Default value: true\r\n" +
        "IndoorAndSunroom = true\r\n" +
        "Terrace = true\r\n" +
        "Basement = maybe\r\n";

    [Fact]
    public void The_players_pool_switches_carry_over()
    {
        var values = OldPoolConfig.Read(OldConfig);

        Assert.True(values["IndoorAndSunroom"]);
        Assert.True(values["Terrace"]);
    }

    [Fact]
    public void Values_outside_the_pool_section_or_not_true_or_false_are_ignored()
    {
        var values = OldPoolConfig.Read(OldConfig);

        Assert.False(values.ContainsKey("Basement"));
        Assert.Equal(2, values.Count);
    }
}
