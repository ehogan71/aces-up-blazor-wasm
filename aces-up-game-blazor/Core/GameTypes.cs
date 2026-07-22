namespace aces_up_game_blazor.Core;

public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

public enum PlayableAction
{
    Discard,
    Relocate
}

public enum GameOutcome
{
    Won,
    Lost
}

public sealed record Card(Suit Suit, int Rank)
{
    public string SuitSymbol => Suit switch
    {
        Suit.Hearts => "H",
        Suit.Diamonds => "D",
        Suit.Clubs => "C",
        _ => "S"
    };

    public string ImagePath => $"cards/{SuitSymbol}-{Rank}.png";
}

public sealed class GameState
{
    public List<List<Card>> Piles { get; init; } = [[], [], [], []];
    public List<Card> DrawPile { get; init; } = [];
    public List<Card> DiscardPile { get; init; } = [];

    public GameState DeepClone()
    {
        return new GameState
        {
            Piles = Piles.Select(pile => pile.Select(card => card with { }).ToList()).ToList(),
            DrawPile = DrawPile.Select(card => card with { }).ToList(),
            DiscardPile = DiscardPile.Select(card => card with { }).ToList()
        };
    }
}
