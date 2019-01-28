using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlackjackStrategy.Models
{
    class Card
    {
        // the card attribute enums
        public enum Suits { Hearts, Spades, Clubs, Diamonds };
        public enum Ranks { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace };

        // this card
        public Ranks Rank { get; set; }
        public Suits Suit { get; set; }

        private static List<Ranks> rankList;
        private static List<Suits> suitList;

        public Card(Ranks rankValue, Suits suit)
        {
            Rank = rankValue;
            Suit = suit;
        }

        public Card(int rankValue, Suits suit)
        {
            Rank = (Ranks) rankValue;
            Suit = suit;
        }

        public static List<Ranks> ListOfRanks
        {
            get
            {
                if (rankList != null) return rankList;

                var ranks = Enum.GetValues(typeof(Ranks));
                var result = new List<Ranks>();
                foreach (var rank in ranks)
                    result.Add((Ranks)rank);
                rankList = result;
                return result;
            }
        }

        public static List<Suits> ListOfSuits
        {
            get
            {
                if (suitList != null) return suitList;

                var suits = Enum.GetValues(typeof(Suits));
                var result = new List<Suits>();
                foreach (var suit in suits)
                    result.Add((Suits)suit);
                suitList = result;
                return result;
            }
        }

        public int RankValueHigh
        {
            get
            {
                switch (Rank)
                {
                    case Ranks.Ace:
                        return 11;

                    case Ranks.King:
                    case Ranks.Queen:
                    case Ranks.Jack:
                    case Ranks.Ten:
                        return 10;

                    default:
                        return Convert.ToInt32(Rank);
                }
            }
        }

        public int RankValueLow
        {
            get
            {
                switch (Rank)
                {
                    case Ranks.Ace:
                        return 1;

                    case Ranks.King:
                    case Ranks.Queen:
                    case Ranks.Jack:
                    case Ranks.Ten:
                        return 10;

                    default:
                        return Convert.ToInt32(Rank);
                }
            }
        }

        public static string RankText(Ranks rank)
        {
            var rankChars = "  23456789TJQKA".ToCharArray();
            return rankChars[Convert.ToInt32(rank)].ToString();
        }

        public override string ToString()
        {
            return RankText(Rank) + Suit;
        }
    }

    //=======================================================================

    class Hand
    {
        public List<Card> Cards { get; set; }

        public Hand()
        {
            Cards = new List<Card>();
        }

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public override string ToString()
        {
            List<string> cardNames = new List<string>();
            foreach (var card in Cards)
                cardNames.Add(card.ToString());

            string hand = String.Join(",", cardNames);
            return hand + " = " + HandValue().ToString();
        }

        public bool IsPair()
        {
            if (Cards.Count > 2) return false;
            return (Cards[0].Rank == Cards[1].Rank);
        }

        public bool HasSoftAce()
        {
            // first, we need to have an ace
            if (!Cards.Any(c => c.Rank == Card.Ranks.Ace)) return false;

            // and if it counts as 11 and we have a valid hand, then we have a soft ace
            int highTotal = Cards.Sum(c => c.RankValueHigh);
            return (highTotal <= 21);
        }

        public int HandValue()
        {
            // the best score possible
            int highValue = 0, lowValue = 0;
            bool aceWasUsedAsHigh = false;
            foreach (var card in Cards)
            {
                if (card.Rank == Card.Ranks.Ace)
                {
                    if (!aceWasUsedAsHigh)
                    {
                        highValue += card.RankValueHigh;
                        lowValue += card.RankValueLow;
                        aceWasUsedAsHigh = true;
                    }
                    else
                    {
                        // only one Ace can be used as high, so all others are low
                        highValue += card.RankValueLow;
                        lowValue += card.RankValueLow;
                    }

                }
                else
                {
                    highValue += card.RankValueHigh;
                    lowValue += card.RankValueLow;
                }
            }

            // if the low value > 21, then so is the high, so simply pass back the low
            if (lowValue > 21) return lowValue;

            // if the high value > 21, return the low
            if (highValue > 21) return lowValue;
            // else the high, which will be the same value as the low except when there's an Ace in the hand
            return highValue;
        }
    }

    //=======================================================================

    class MultiDeck
    {
        public List<Card> Cards { get; set; }
        private int currentCard = 0;

        public MultiDeck(int numDecks)
        {
            Cards = CardUtils.GetRandomDeck(numDecks);
        }

        public Card DealCard()
        {
            ShuffleIfNeeded();

            //Debug.WriteLine("Dealing card from " + this.ToString());
            Debug.Assert(currentCard < Cards.Count, "Ran out of cards to deal");

            // bad code - it doesn't deal with running out of cards
            return Cards[currentCard++];
        }

        public void ForceNextCardToBe(Card.Ranks rank)
        {
            // to compensate for the fact that pairs and soft hands don't happen that often,
            // we may wish to force a card to be dealt next

            // first, look for the rank in the remaining cards
            int foundAt = -1;
            for (int i = currentCard; i < Cards.Count; i++)
                if (Cards[i].Rank == rank)
                {
                    foundAt = i;
                    break;
                }

            // if not found, start over from the start of the deck
            if (foundAt == -1)
            {
                for (int i = 0; i < currentCard; i++)
                    if (Cards[i].Rank == rank)
                    {
                        foundAt = i;
                        break;
                    }
            }

            // now swap that card with the next-to-be-dealt
            Card temp = Cards[foundAt];
            Cards[foundAt] = Cards[currentCard];
            Cards[currentCard] = temp;
        }

        public void EnsureNextCardIsnt(Card.Ranks rank)
        {
            // similar to ForceNextCardToBe, this is used for stacking the deck
            while (Cards[currentCard].Rank == rank)
            {
                currentCard++;
                ShuffleIfNeeded();
            }
        }

        public int CardsRemaining {
            get
            {
                return Cards.Count - currentCard;
            }
        }

        public override string ToString()
        {
            return CardsRemaining + " remaining, first cards are " +
                Cards[0].ToString() + " " + Cards[1].ToString() + " " + Cards[2].ToString();
        }

        public void Shuffle()
        {
            // then shuffle using Fisher-Yates: one pass through, swapping the current card with a random one below it
            int start = Cards.Count - 1;
            for (int i = start; i > 1; i--)
            {
                int swapWith = Randomizer.IntLessThan(i);

                Card hold = Cards[i];
                Cards[i] = Cards[swapWith];
                Cards[swapWith] = hold;
            }
            currentCard = 0;
        }

        public void ShuffleIfNeeded()
        {
            if (CardsRemaining < 20)
                Shuffle();
        }

    }

    //=======================================================================

    class CardUtils
    {
        static public List<Card> GetRandomDeck(int numDecks)
        {
            // initially populate
            List<Card> deck = new List<Card>(52 * numDecks);

            for (int i = 0; i < numDecks; i++)
                foreach (var rank in Card.ListOfRanks)
                    foreach (var suit in Card.ListOfSuits)
                    {
                        var card = new Card(rank, suit);
                        deck.Add(card);
                    }

            // then shuffle using Fisher-Yates: one pass through, swapping the current card with a random one below it
            int start = deck.Count - 1;
            for (int i = start; i > 1; i--)
            {
                int swapWith = Randomizer.IntLessThan(i);

                Card hold = deck[i];
                deck[i] = deck[swapWith];
                deck[swapWith] = hold;
            }

            return deck;
        }
    }
}
