namespace aces_up_game_blazor.Core;

public static class GameEngine
{
    private static readonly Suit[] Suits = [Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades];
    private static readonly int[] Ranks = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13];

    public static GameState CreateInitialState(Random random)
    {
        return new GameState
        {
            Piles = [[], [], [], []],
            DrawPile = CreateDeck(random),
            DiscardPile = []
        };
    }

    public static List<Card> CreateDeck(Random random)
    {
        var deck = new List<Card>(52);

        foreach (var suit in Suits)
        {
            foreach (var rank in Ranks)
            {
                deck.Add(new Card(suit, rank));
            }
        }

        // Fisher-Yates shuffle.
        for (var i = deck.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }

        return deck;
    }

    public static (GameState NewState, List<Card> DealtCards) DealCards(GameState state)
    {
        if (state.DrawPile.Count < 4)
        {
            return (state.DeepClone(), []);
        }

        var newState = state.DeepClone();
        var dealt = new List<Card>(4);

        for (var i = 0; i < 4; i++)
        {
            var card = newState.DrawPile[^1];
            newState.DrawPile.RemoveAt(newState.DrawPile.Count - 1);
            newState.Piles[i].Add(card);
            dealt.Add(card);
        }

        return (newState, dealt);
    }

    public static (GameState NewState, Card? DiscardedCard) DiscardTopCard(GameState state, int pileIndex)
    {
        if (pileIndex < 0 || pileIndex >= state.Piles.Count || state.Piles[pileIndex].Count == 0)
        {
            return (state.DeepClone(), null);
        }

        var newState = state.DeepClone();
        var pile = newState.Piles[pileIndex];
        var discarded = pile[^1];

        pile.RemoveAt(pile.Count - 1);
        newState.DiscardPile.Add(discarded);

        return (newState, discarded);
    }

    public static (GameState NewState, Card? MovedCard, int? TargetPileIndex) RelocateTopCard(GameState state, int pileIndex)
    {
        if (pileIndex < 0 || pileIndex >= state.Piles.Count || state.Piles[pileIndex].Count == 0)
        {
            return (state.DeepClone(), null, null);
        }

        var targetIndex = state.Piles.FindIndex((pile, index) => index != pileIndex && pile.Count == 0);
        if (targetIndex == -1)
        {
            return (state.DeepClone(), null, null);
        }

        var newState = state.DeepClone();
        var sourcePile = newState.Piles[pileIndex];
        var moved = sourcePile[^1];
        sourcePile.RemoveAt(sourcePile.Count - 1);
        newState.Piles[targetIndex].Add(moved);

        return (newState, moved, targetIndex);
    }

    public static PlayableAction? GetTopCardPlayableAction(GameState state, int pileIndex)
    {
        if (pileIndex < 0 || pileIndex >= state.Piles.Count)
        {
            return null;
        }

        var pile = state.Piles[pileIndex];
        if (pile.Count == 0)
        {
            return null;
        }

        var topCard = pile[^1];

        var hasHigherMatchingSuit = state.Piles.Any((otherPile, otherIndex) =>
        {
            if (otherIndex == pileIndex || otherPile.Count == 0)
            {
                return false;
            }

            var otherTop = otherPile[^1];
            return otherTop.Suit == topCard.Suit && IsHigherRank(otherTop, topCard);
        });

        if (hasHigherMatchingSuit)
        {
            return PlayableAction.Discard;
        }

        var hasEmptyPile = state.Piles.Any((otherPile, otherIndex) =>
            otherIndex != pileIndex && otherPile.Count == 0);

        if (pile.Count == 1 && hasEmptyPile)
        {
            return null;
        }

        return hasEmptyPile ? PlayableAction.Relocate : null;
    }

    public static GameOutcome? GetGameOutcome(GameState state)
    {
        if (state.DrawPile.Count > 0)
        {
            return null;
        }

        var hasPlayableCard = state.Piles.Any((_, index) => GetTopCardPlayableAction(state, index) is not null);
        if (hasPlayableCard)
        {
            return null;
        }

        var won = state.Piles.All(pile => pile.Count == 1 && pile[0].Rank == 1);
        return won ? GameOutcome.Won : GameOutcome.Lost;
    }

    private static int RankValue(int rank) => rank == 1 ? 14 : rank;

    private static bool IsHigherRank(Card candidate, Card target) => RankValue(candidate.Rank) > RankValue(target.Rank);

    private static bool Any<T>(this IReadOnlyList<T> source, Func<T, int, bool> predicate)
    {
        for (var i = 0; i < source.Count; i++)
        {
            if (predicate(source[i], i))
            {
                return true;
            }
        }

        return false;
    }

    private static bool All<T>(this IReadOnlyList<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            if (!predicate(item))
            {
                return false;
            }
        }

        return true;
    }

    private static int FindIndex<T>(this IReadOnlyList<T> source, Func<T, int, bool> predicate)
    {
        for (var i = 0; i < source.Count; i++)
        {
            if (predicate(source[i], i))
            {
                return i;
            }
        }

        return -1;
    }
}
