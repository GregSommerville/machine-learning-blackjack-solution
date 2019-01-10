using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlackjackStrategy.Models
{
    class Card
    {
        public string Rank { get; set; }
        public string Suit { get; set; }

        public enum Suits {  Hearts, Spades, Clubs, Diamonds};
        public enum Ranks {  Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace};

        public int RankValueHigh
        {
            get
            {
                switch (Rank)
                {
                    case "A":
                        return 11;
                    case "K":
                        return 10;
                    case "Q":
                        return 10;
                    case "J":
                        return 10;
                    case "T":
                        return 10;
                    default:
                        return Convert.ToInt32(Rank);
                }
            }
        }

        public int RankValueLow
        {
            // For straights where A is treated as below the 2
            get
            {
                switch (Rank)
                {
                    case "A":
                        return 1;
                    case "K":
                        return 10;
                    case "Q":
                        return 10;
                    case "J":
                        return 10;
                    case "T":
                        return 10;
                    default:
                        return Convert.ToInt32(Rank);
                }
            }
        }

        public Card(string rank, string suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public Card(string combined)
        {
            Rank = combined[0].ToString();
            Suit = combined[1].ToString();
        }

        public Card(int rankValue, string suit)
        {
            var rankChars = "  23456789TJQKA".ToCharArray();
            Rank = rankChars[rankValue].ToString();
            Suit = suit;
        }

        public Card(Ranks rankValue, string suit)
        {
            var rankChars = "  23456789TJQKA".ToCharArray();
            Rank = rankChars[(int)rankValue].ToString();
            Suit = suit;
        }

        public Card(Ranks rankValue, Suits suit)
        {
            var rankChars = "  23456789TJQKA".ToCharArray();
            var suitChars = "HSCD";
            Rank = rankChars[(int)rankValue].ToString();
            Suit = suitChars[(int)suit].ToString();
        }

        public static string RankText(int rank)
        {
            var rankChars = "  23456789TJQKA".ToCharArray();
            return rankChars[rank].ToString();
        }

        public static string RankDescription(int rank, bool pluralForm = true)
        {
            string suffix = "";
            if (pluralForm)
            {
                suffix = "s";
                if (rank == 6) suffix = "es";
            }

            switch (rank)
            {
                case 14:
                    return "Ace" + suffix;
                case 13:
                    return "King" + suffix;
                case 12:
                    return "Queen" + suffix;
                case 11:
                    return "Jack" + suffix;
                case 10:
                    return "Ten" + suffix;
                case 9:
                    return "Nine" + suffix;
                case 8:
                    return "Eight" + suffix;
                case 7:
                    return "Seven" + suffix;
                case 6:
                    return "Six" + suffix;
                case 5:
                    return "Five" + suffix;
                case 4:
                    return "Four" + suffix;
                case 3:
                    return "Three" + suffix;
                case 2:
                    return "Two" + suffix;
            }
            return "";
        }

        public override string ToString()
        {
            return Rank + Suit;
        }
    }

    //=======================================================================

    class Hand
    {
        public Hand()
        {
            Cards = new List<Card>();
        }

        public List<Card> Cards { get; set; }

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
            if (!Cards.Any(c => c.Rank == "A")) return false;

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
                if (card.Rank == "A")
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
            Cards = new List<Card>();
            for (int deckNum = 0; deckNum < numDecks; deckNum++)
            {
                Cards.AddRange(CardUtils.GetRandomDeck());
            }
        }

        public Card DealCard()
        {
            //Debug.WriteLine("Dealing card from " + this.ToString());
            Debug.Assert(currentCard < Cards.Count, "Ran out of cards to deal");

            // bad code - it doesn't deal with running out of cards
            return Cards[currentCard++];
        }

        internal Card DealNextOfRank(string rank)
        {
            int index = currentCard;
            while (Cards[index].Rank != rank) index++;
            var card = Cards[index];
            Cards.Remove(card);
            return card;
        }

        internal Card DealNextNotOfRank(string rank)
        {
            int index = currentCard;
            while (Cards[index].Rank == rank) index++;
            var card = Cards[index];
            Cards.Remove(card);
            return card;
        }

        internal void RemoveCard(string rank, string suit)
        {
            if (rank == "10") rank = "T";
            var foundCard = Cards.First(c => c.Rank == rank && c.Suit == suit);
            Cards.Remove(foundCard);
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

    }

    //=======================================================================

    class CardUtils
    {
        static public List<Card> GetRandomDeck()
        {
            // initially populate
            List<Card> deck = new List<Card>(52);
            foreach (Card.Ranks rank in Enum.GetValues(typeof(Card.Ranks)))
                foreach (Card.Suits suit in Enum.GetValues(typeof(Card.Suits)))
                {
                    var card = new Card(rank, suit);
                    deck.Add(card);
                }

            // then shuffle using Fisher-Yates: one pass through, swapping the current card with a random one below it
            for (int i = 51; i > 1; i--)
            {
                int swapWith = Randomizer.IntLessThan(i);

                Card hold = deck[i];
                deck[i] = deck[swapWith];
                deck[swapWith] = hold;
            }

            return deck;
        }

        static public List<Card> GetRandomCards(int numCards)
        {
            // optimized way of getting a full deck
            if (numCards == 52)
                return GetRandomDeck();

            string suits = "HSCD";
            string ranks = "23456789TJQKA";

            List<Card> cards = new List<Card>(numCards);
            string suit, rank;
            while (cards.Count < numCards)
            {
                // Generate a card and make sure we don't already have it 
                do
                {
                    suit = suits[Randomizer.IntLessThan(4)].ToString();
                    rank = ranks[Randomizer.IntLessThan(13)].ToString();
                } while (cards.Any(c => c.Rank == rank && c.Suit == suit));

                cards.Add(new Card(rank, suit));
            }
            return cards;
        }
    }

    //=======================================================================
}
