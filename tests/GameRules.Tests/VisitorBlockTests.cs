using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class VisitorBlockTests
{
    private const int DoorPounding = 20225;
    private const int MidnightVisitorsSmallLoan = 20210;
    private const int RevengeNight = 20236;
    private const int StalkNight = 20241;
    private const int LoanWarningText = 20222;
    private const int ColdWaveStrikes = 20200;
    private const int WellDressedVisitor = 20230;

    [Fact]
    public void The_games_rules_turn_no_one_away()
    {
        Assert.False(VisitorBlock.Game.BlocksAny);
        Assert.False(VisitorBlock.Game.BlocksEvent(DoorPounding));
        Assert.False(VisitorBlock.Game.BlocksEvent(MidnightVisitorsSmallLoan));
        Assert.False(VisitorBlock.Game.BlocksText(LoanWarningText));
    }

    [Fact]
    public void No_door_smasher_stops_the_door_pounding_but_not_the_loan_shark()
    {
        var block = new VisitorBlock(NoDoorSmasher: true, NoDebtCollector: false);

        Assert.True(block.BlocksEvent(DoorPounding));
        Assert.False(block.BlocksEvent(MidnightVisitorsSmallLoan));
        Assert.False(block.BlocksText(LoanWarningText));
    }

    [Fact]
    public void No_debt_collector_stops_every_loan_shark_visit_and_its_warning_text()
    {
        var block = new VisitorBlock(NoDoorSmasher: false, NoDebtCollector: true);

        Assert.True(block.BlocksEvent(MidnightVisitorsSmallLoan));
        Assert.True(block.BlocksEvent(RevengeNight));
        Assert.True(block.BlocksEvent(StalkNight));
        Assert.True(block.BlocksText(LoanWarningText));
        Assert.False(block.BlocksEvent(DoorPounding));
    }

    [Fact]
    public void Other_events_still_happen_with_both_switches_on()
    {
        var block = new VisitorBlock(NoDoorSmasher: true, NoDebtCollector: true);

        Assert.False(block.BlocksEvent(ColdWaveStrikes));
        Assert.False(block.BlocksEvent(WellDressedVisitor));
    }
}
