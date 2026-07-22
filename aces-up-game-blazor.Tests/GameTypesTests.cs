using Xunit;
using aces_up_game_blazor.Core;

namespace aces_up_game_blazor.Tests;

public sealed class GameTypesTests
{
    [Theory]
    [InlineData(Suit.Hearts, "H", "cards/H-1.png")]
    [InlineData(Suit.Diamonds, "D", "cards/D-10.png")]
    [InlineData(Suit.Clubs, "C", "cards/C-13.png")]
    [InlineData(Suit.Spades, "S", "cards/S-7.png")]
    public void CardProperties_ExposeExpectedValues(Suit suit, string expectedSymbol, string expectedImagePath)
    {
        var rank = expectedImagePath.Contains("-10") ? 10 : expectedImagePath.Contains("-13") ? 13 : expectedImagePath.Contains("-7") ? 7 : 1;
        var card = new Card(suit, rank);

        Assert.Equal(expectedSymbol, card.SuitSymbol);
        Assert.Equal(expectedImagePath, card.ImagePath);
    }

    [Fact]
    public void DeepClone_CreatesIndependentNestedCollections()
    {
        var original = new GameState
        {
            Piles =
            [
                [new Card(Suit.Hearts, 1), new Card(Suit.Diamonds, 2)],
                [new Card(Suit.Clubs, 3)],
                [],
                [new Card(Suit.Spades, 4)]
            ],
            DrawPile = [new Card(Suit.Hearts, 5)],
            DiscardPile = [new Card(Suit.Diamonds, 6)]
        };

        var clone = original.DeepClone();

        Assert.NotSame(original, clone);
        Assert.NotSame(original.Piles, clone.Piles);
        Assert.NotSame(original.DrawPile, clone.DrawPile);
        Assert.NotSame(original.DiscardPile, clone.DiscardPile);
        Assert.Equal(original.Piles, clone.Piles);
        Assert.Equal(original.DrawPile, clone.DrawPile);
        Assert.Equal(original.DiscardPile, clone.DiscardPile);
        Assert.NotSame(original.Piles[0][0], clone.Piles[0][0]);

        clone.Piles[0].RemoveAt(0);
        clone.DrawPile.Add(new Card(Suit.Clubs, 7));

        Assert.Equal(2, original.Piles[0].Count);
        Assert.Single(original.DrawPile);
        Assert.True(clone.Piles[0].Count == 1);
        Assert.True(clone.DrawPile.Count == 2);
    }
}