using Xunit;
using aces_up_game_blazor.Core;

namespace aces_up_game_blazor.Tests;

public sealed class GameEngineTests
{
    [Fact]
    public void CreateDeck_ReturnsFiftyTwoUniqueCards()
    {
        var deck = GameEngine.CreateDeck(new Random(1234));

        Assert.Equal(52, deck.Count);
        Assert.True(deck.Distinct().Count() == 52);
        Assert.Contains(deck, card => card is { Suit: Suit.Hearts, Rank: 1 });
        Assert.Contains(deck, card => card is { Suit: Suit.Spades, Rank: 13 });
    }

    [Fact]
    public void CreateInitialState_UsesFourEmptyPilesAndFullDeck()
    {
        var state = GameEngine.CreateInitialState(new Random(1));

        Assert.Equal(4, state.Piles.Count);
        Assert.All(state.Piles, pile => Assert.Empty(pile));
        Assert.Equal(52, state.DrawPile.Count);
        Assert.Empty(state.DiscardPile);
    }

    [Fact]
    public void DealCards_WhenFewerThanFourCards_ReturnsCloneAndNoDealtCards()
    {
        var state = CreateState(
            drawPile: [
                new Card(Suit.Hearts, 1),
                new Card(Suit.Diamonds, 2),
                new Card(Suit.Clubs, 3)
            ]);

        var (newState, dealtCards) = GameEngine.DealCards(state);

        Assert.NotSame(state, newState);
        Assert.Empty(dealtCards);
        Assert.Equal(state.DrawPile, newState.DrawPile);
        Assert.Equal(state.Piles, newState.Piles);
    }

    [Fact]
    public void DealCards_MovesTopFourCardsToTheFourPiles()
    {
        var state = CreateState(
            drawPile: [
                new Card(Suit.Hearts, 1),
                new Card(Suit.Diamonds, 2),
                new Card(Suit.Clubs, 3),
                new Card(Suit.Spades, 4),
                new Card(Suit.Hearts, 5)
            ]);

        var (newState, dealtCards) = GameEngine.DealCards(state);

        Assert.Equal([
            new Card(Suit.Hearts, 5),
            new Card(Suit.Spades, 4),
            new Card(Suit.Clubs, 3),
            new Card(Suit.Diamonds, 2)
        ], dealtCards);
        Assert.Equal([new Card(Suit.Hearts, 1)], newState.DrawPile);
        Assert.Equal([new Card(Suit.Hearts, 5)], newState.Piles[0]);
        Assert.Equal([new Card(Suit.Spades, 4)], newState.Piles[1]);
        Assert.Equal([new Card(Suit.Clubs, 3)], newState.Piles[2]);
        Assert.Equal([new Card(Suit.Diamonds, 2)], newState.Piles[3]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void DiscardTopCard_InvalidPileIndex_ReturnsCloneAndNull(int pileIndex)
    {
        var state = CreateState(pile0: [new Card(Suit.Hearts, 1)]);

        var (newState, discardedCard) = GameEngine.DiscardTopCard(state, pileIndex);

        Assert.NotSame(state, newState);
        Assert.Null(discardedCard);
        Assert.Equal(state.Piles, newState.Piles);
        Assert.Equal(state.DiscardPile, newState.DiscardPile);
    }

    [Fact]
    public void DiscardTopCard_EmptyPile_ReturnsCloneAndNull()
    {
        var state = CreateState();

        var (newState, discardedCard) = GameEngine.DiscardTopCard(state, 0);

        Assert.NotSame(state, newState);
        Assert.Null(discardedCard);
        Assert.Equal(state.Piles, newState.Piles);
    }

    [Fact]
    public void DiscardTopCard_MovesCardToDiscardPile()
    {
        var topCard = new Card(Suit.Hearts, 9);
        var state = CreateState(pile0: [new Card(Suit.Clubs, 2), topCard]);

        var (newState, discardedCard) = GameEngine.DiscardTopCard(state, 0);

        Assert.Equal(topCard, discardedCard);
        Assert.Equal([new Card(Suit.Clubs, 2)], newState.Piles[0]);
        Assert.Equal([topCard], newState.DiscardPile);
        Assert.Equal([new Card(Suit.Clubs, 2), topCard], state.Piles[0]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void RelocateTopCard_InvalidPileIndex_ReturnsCloneAndNull(int pileIndex)
    {
        var state = CreateState(pile0: [new Card(Suit.Hearts, 1)]);

        var (newState, movedCard, targetIndex) = GameEngine.RelocateTopCard(state, pileIndex);

        Assert.NotSame(state, newState);
        Assert.Null(movedCard);
        Assert.Null(targetIndex);
    }

    [Fact]
    public void RelocateTopCard_EmptySourcePile_ReturnsCloneAndNull()
    {
        var state = CreateState();

        var (newState, movedCard, targetIndex) = GameEngine.RelocateTopCard(state, 0);

        Assert.NotSame(state, newState);
        Assert.Null(movedCard);
        Assert.Null(targetIndex);
    }

    [Fact]
    public void RelocateTopCard_NoEmptyPile_ReturnsCloneAndNull()
    {
        var state = CreateState(
            pile0: [new Card(Suit.Hearts, 5)],
            pile1: [new Card(Suit.Diamonds, 6)],
            pile2: [new Card(Suit.Clubs, 7)],
            pile3: [new Card(Suit.Spades, 8)]);

        var (newState, movedCard, targetIndex) = GameEngine.RelocateTopCard(state, 0);

        Assert.NotSame(state, newState);
        Assert.Null(movedCard);
        Assert.Null(targetIndex);
    }

    [Fact]
    public void RelocateTopCard_MovesCardToFirstEmptyPile()
    {
        var movedCard = new Card(Suit.Hearts, 12);
        var state = CreateState(
            pile0: [new Card(Suit.Clubs, 2), movedCard],
            pile1: [new Card(Suit.Diamonds, 3)],
            pile2: [],
            pile3: [new Card(Suit.Spades, 4)]);

        var (newState, actualMovedCard, targetIndex) = GameEngine.RelocateTopCard(state, 0);

        Assert.Equal(movedCard, actualMovedCard);
        Assert.Equal(2, targetIndex);
        Assert.Equal([new Card(Suit.Clubs, 2)], newState.Piles[0]);
        Assert.Equal([movedCard], newState.Piles[2]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void GetTopCardPlayableAction_InvalidPileIndex_ReturnsNull(int pileIndex)
    {
        var state = CreateState();

        Assert.Null(GameEngine.GetTopCardPlayableAction(state, pileIndex));
    }

    [Fact]
    public void GetTopCardPlayableAction_EmptyPile_ReturnsNull()
    {
        var state = CreateState();

        Assert.Null(GameEngine.GetTopCardPlayableAction(state, 0));
    }

    [Fact]
    public void GetTopCardPlayableAction_ReturnsDiscardWhenHigherMatchingSuitExists()
    {
        var state = CreateState(
            pile0: [new Card(Suit.Clubs, 5)],
            pile1: [new Card(Suit.Clubs, 9)],
            pile2: [new Card(Suit.Diamonds, 2)],
            pile3: [new Card(Suit.Spades, 3)]);

        Assert.Equal(PlayableAction.Discard, GameEngine.GetTopCardPlayableAction(state, 0));
    }

    [Fact]
    public void GetTopCardPlayableAction_TreatsAceAsHighestRank()
    {
        var state = CreateState(
            pile0: [new Card(Suit.Hearts, 13)],
            pile1: [new Card(Suit.Hearts, 1)],
            pile2: [new Card(Suit.Diamonds, 2)],
            pile3: [new Card(Suit.Spades, 3)]);

        Assert.Equal(PlayableAction.Discard, GameEngine.GetTopCardPlayableAction(state, 0));
    }

    [Fact]
    public void GetTopCardPlayableAction_ReturnsNullForSingleCardPileWithEmptyPile()
    {
        var state = CreateState(
            pile0: [new Card(Suit.Hearts, 7)],
            pile1: [],
            pile2: [new Card(Suit.Diamonds, 4)],
            pile3: [new Card(Suit.Spades, 8)]);

        Assert.Null(GameEngine.GetTopCardPlayableAction(state, 0));
    }

    [Fact]
    public void GetTopCardPlayableAction_ReturnsRelocateWhenEmptyPileExistsAndNoDiscardIsAvailable()
    {
        var state = CreateState(
            pile0: [new Card(Suit.Hearts, 7), new Card(Suit.Clubs, 9)],
            pile1: [],
            pile2: [new Card(Suit.Diamonds, 4)],
            pile3: [new Card(Suit.Spades, 8)]);

        Assert.Equal(PlayableAction.Relocate, GameEngine.GetTopCardPlayableAction(state, 0));
    }

    [Fact]
    public void GetTopCardPlayableAction_ReturnsNullWhenNoEmptyPileAndNoDiscardAvailable()
    {
        var state = CreateState(
            pile0: [new Card(Suit.Hearts, 7), new Card(Suit.Clubs, 9)],
            pile1: [new Card(Suit.Diamonds, 4)],
            pile2: [new Card(Suit.Spades, 8)],
            pile3: [new Card(Suit.Diamonds, 12)]);

        Assert.Null(GameEngine.GetTopCardPlayableAction(state, 0));
    }

    [Fact]
    public void GetGameOutcome_ReturnsNullWhenDrawPileHasCards()
    {
        var state = CreateState(drawPile: [new Card(Suit.Hearts, 1)]);

        Assert.Null(GameEngine.GetGameOutcome(state));
    }

    [Fact]
    public void GetGameOutcome_ReturnsNullWhenAnyPlayableCardExists()
    {
        var state = CreateState(
            pile0: [new Card(Suit.Clubs, 5)],
            pile1: [new Card(Suit.Clubs, 9)],
            pile2: [new Card(Suit.Diamonds, 2)],
            pile3: [new Card(Suit.Spades, 3)]);

        Assert.Null(GameEngine.GetGameOutcome(state));
    }

    [Fact]
    public void GetGameOutcome_ReturnsWonWhenEachPileContainsOnlyAnAce()
    {
        var state = CreateState(
            pile0: [new Card(Suit.Hearts, 1)],
            pile1: [new Card(Suit.Diamonds, 1)],
            pile2: [new Card(Suit.Clubs, 1)],
            pile3: [new Card(Suit.Spades, 1)]);

        Assert.Equal(GameOutcome.Won, GameEngine.GetGameOutcome(state));
    }

    [Fact]
    public void GetGameOutcome_ReturnsLostWhenNoMovesRemainAndNotAllPilesAreAces()
    {
        var state = CreateState(
            pile0: [new Card(Suit.Hearts, 13)],
            pile1: [new Card(Suit.Diamonds, 12)],
            pile2: [new Card(Suit.Clubs, 11)],
            pile3: [new Card(Suit.Spades, 10)]);

        Assert.Equal(GameOutcome.Lost, GameEngine.GetGameOutcome(state));
    }

    private static GameState CreateState(
        List<Card>? pile0 = null,
        List<Card>? pile1 = null,
        List<Card>? pile2 = null,
        List<Card>? pile3 = null,
        List<Card>? drawPile = null,
        List<Card>? discardPile = null)
    {
        return new GameState
        {
            Piles = [pile0 ?? [], pile1 ?? [], pile2 ?? [], pile3 ?? []],
            DrawPile = drawPile ?? [],
            DiscardPile = discardPile ?? []
        };
    }
}