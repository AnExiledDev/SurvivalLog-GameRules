using GameRules.Core;
using Xunit;

namespace GameRules.Tests;

public sealed class PlagueBlockTests
{
    private const int FeelingUnwell = 40600;
    private const int Outbreak = 40603;
    private const int DiseaseWorsens = 40602;
    private const int EpidemicSubsides = 40601;
    private const int SecondCharacterFeelingUnwell = 52600;
    private const int ThirdCharacterDiseaseWorsens = 62602;
    private const int UnrelatedEvent = 20225;

    [Fact]
    public void The_games_rules_stop_no_plague_event()
    {
        var block = PlagueBlock.Game;

        Assert.False(block.BlocksAny);
        Assert.False(block.BlocksEvent(FeelingUnwell, NoPlagueYet));
        Assert.False(block.BlocksEvent(DiseaseWorsens, PlagueUnderway));
        Assert.False(block.BlocksEvent(EpidemicSubsides, NoPlagueYet));
    }

    [Fact]
    public void No_plague_stops_the_start_the_outbreak_and_the_worsening_for_every_character()
    {
        var block = new PlagueBlock(NoPlague: true, NoWorsening: false);

        Assert.True(block.BlocksEvent(FeelingUnwell, NoPlagueYet));
        Assert.True(block.BlocksEvent(Outbreak, PlagueUnderway));
        Assert.True(block.BlocksEvent(DiseaseWorsens, PlagueUnderway));
        Assert.True(block.BlocksEvent(SecondCharacterFeelingUnwell, NoPlagueYet));
        Assert.True(block.BlocksEvent(ThirdCharacterDiseaseWorsens, PlagueUnderway));
    }

    [Fact]
    public void No_plague_pays_no_ending_reward_for_a_plague_that_never_came()
    {
        var block = new PlagueBlock(NoPlague: true, NoWorsening: false);

        Assert.True(block.BlocksEvent(EpidemicSubsides, NoPlagueYet));
    }

    [Fact]
    public void A_plague_already_underway_still_ends_so_its_illness_clears()
    {
        var block = new PlagueBlock(NoPlague: true, NoWorsening: false);

        Assert.False(block.BlocksEvent(EpidemicSubsides, PlagueUnderway));
    }

    [Fact]
    public void No_worsening_stops_only_the_worsening()
    {
        var block = new PlagueBlock(NoPlague: false, NoWorsening: true);

        Assert.True(block.BlocksEvent(DiseaseWorsens, PlagueUnderway));
        Assert.True(block.BlocksEvent(ThirdCharacterDiseaseWorsens, PlagueUnderway));
        Assert.False(block.BlocksEvent(FeelingUnwell, NoPlagueYet));
        Assert.False(block.BlocksEvent(Outbreak, PlagueUnderway));
        Assert.False(block.BlocksEvent(EpidemicSubsides, NoPlagueYet));
    }

    [Fact]
    public void Events_outside_the_plague_are_never_stopped()
    {
        var block = new PlagueBlock(NoPlague: true, NoWorsening: true);

        Assert.False(block.BlocksEvent(UnrelatedEvent, NoPlagueYet));
    }

    private static bool NoPlagueYet() => false;

    private static bool PlagueUnderway() => true;
}
