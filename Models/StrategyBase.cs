using System;
using System.Linq;

namespace BlackjackStrategy.Models
{
    
    // enums and other public constants
    public enum ActionToTake { Stand, Hit, Double, Split };

    abstract class StrategyBase
    {
        public static int NumActionsNoSplit = 3;
        public static int NumActionsWithSplit = 4;

        public static int HighestUpcardIndex = 9;

        // soft remainders go from 2 to 9, since A-10 is blackjack, and A-1 isn't possible (would be A-A)
        public static int LowestSoftHandRemainder = 2;
        public static int HighestSoftHandRemainder = 9;

        // hard hand values go from 5 (since 4 is 2-2) to 20 (since 21 is Blackjack)
        public static int LowestHardHandValue = 5;
        public static int HighestHardHandValue = 20;

        public float Fitness { get; set; } = 0;

        private ActionToTake[,] pairsStrategy, softStrategy, hardStrategy;

        public StrategyBase()
        {
            // everything initialized to "Stand"
            pairsStrategy = new ActionToTake[HighestUpcardIndex + 1, HighestUpcardIndex + 1];
            softStrategy = new ActionToTake[HighestUpcardIndex + 1, HighestSoftHandRemainder + 1];
            hardStrategy = new ActionToTake[HighestUpcardIndex + 1, HighestHardHandValue + 1];
        }

        public void DeepCopy(StrategyBase copyFrom)
        {
            this.Fitness = copyFrom.Fitness;
            pairsStrategy = (ActionToTake[,])copyFrom.pairsStrategy.Clone();
            softStrategy = (ActionToTake[,])copyFrom.softStrategy.Clone();
            hardStrategy = (ActionToTake[,])copyFrom.hardStrategy.Clone();
        }

        // getters and setters for the three tables, used during crossover
        public ActionToTake GetActionForPair(int upcardRank, int pairRank)
        {
            return pairsStrategy[upcardRank, pairRank];
        }
        public void SetActionForPair(int upcardRank, int pairRank, ActionToTake action)
        {
            pairsStrategy[upcardRank, pairRank] = action;
        }

        public ActionToTake GetActionForSoftHand(int upcardRank, int softRemainder)
        {
            return softStrategy[upcardRank, softRemainder];
        }
        public void SetActionForSoftHand(int upcardRank, int softRemainder, ActionToTake action)
        {
            softStrategy[upcardRank, softRemainder] = action;
        }

        public ActionToTake GetActionForHardHand(int upcardRank, int hardTotal)
        {
            return hardStrategy[upcardRank, hardTotal];
        }
        public void SetActionForHardHand(int upcardRank, int hardTotal, ActionToTake action)
        {
            hardStrategy[upcardRank, hardTotal] = action;
        }


        // the final result which we use when testing
        public ActionToTake GetActionForHand(Hand hand, Card dealerUpcard)
        {
            if (hand.HandValue() >= 21) return ActionToTake.Stand;

            var upcardIndex = IndexFromRank(dealerUpcard.Rank);

            if (hand.IsPair())
            {
                var pairIndex = IndexFromRank(hand.Cards[0].Rank);
                return pairsStrategy[upcardIndex, pairIndex];
            }

            if (hand.HasSoftAce())
            {
                // we want the total other than the high ace
                int howManyAces = hand.Cards.Count(c => c.Rank == Card.Ranks.Ace);
                int total = hand.Cards
                    .Where(c => c.Rank != Card.Ranks.Ace)
                    .Sum(c => c.RankValueHigh) +
                    (howManyAces - 1);

                return softStrategy[upcardIndex, total];
            }

            return hardStrategy[upcardIndex, hand.HandValue()];
        }

        public int IndexFromRank(Card.Ranks rank)
        {
            // we collapse certain ranks together because they're the same value
            switch (rank)
            {
                case Card.Ranks.Ace:
                    return 9;

                case Card.Ranks.King:
                case Card.Ranks.Queen:
                case Card.Ranks.Jack:
                case Card.Ranks.Ten:
                    return 8;

                case Card.Ranks.Nine:
                    return 7;

                case Card.Ranks.Eight:
                    return 6;

                case Card.Ranks.Seven:
                    return 5;

                case Card.Ranks.Six:
                    return 4;

                case Card.Ranks.Five:
                    return 3;

                case Card.Ranks.Four:
                    return 2;

                case Card.Ranks.Three:
                    return 1;

                case Card.Ranks.Two:
                    return 0;
            }
            throw new InvalidOperationException();
        }
    }
}
