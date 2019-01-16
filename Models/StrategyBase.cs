using System.Linq;

namespace BlackjackStrategy.Models
{
    // enums and other public constants
    public enum ActionToTake { Stand, Hit, Double, Split };

    abstract class StrategyBase
    {
        public static int NumActionsNoSplit = 3;
        public static int NumActionsWithSplit = 4;

        public static int HighestUpcardRank = (int)Card.Ranks.Ace;
        public static int HighestPairRank = (int)Card.Ranks.Ace;

        // soft remainders go from 2 to 9, since A-10 is blackjack, and A-1 isn't possible (would be A-A)
        public static int LowestSoftHandRemainder = 2;
        public static int HighestSoftHandRemainder = 9;

        // hard hand values go from 5 (since 4 is 2-2) to 20 (since 21 is Blackjack)
        public static int LowestHardHandValue = 5;
        public static int HighestHardHandValue = 20;

        private ActionToTake[,] pairsStrategy, softStrategy, hardStrategy;

        public StrategyBase()
        {
            // everything initialized to "Stand"
            pairsStrategy = new ActionToTake[HighestUpcardRank + 1, HighestPairRank + 1];
            softStrategy = new ActionToTake[HighestUpcardRank + 1, HighestSoftHandRemainder + 1];
            hardStrategy = new ActionToTake[HighestUpcardRank + 1, HighestHardHandValue + 1];
        }

        public void DeepCopy(StrategyBase copyFrom)
        {
            pairsStrategy = (ActionToTake[,])copyFrom.pairsStrategy.Clone();
            softStrategy = (ActionToTake[,])copyFrom.softStrategy.Clone();
            hardStrategy = (ActionToTake[,])copyFrom.hardStrategy.Clone();
        }

        // getters and setters for the three tables, used during crossover
        public ActionToTake GetActionForPair(Card.Ranks upcardRank, Card.Ranks pairRank)
        {
            return pairsStrategy[(int)upcardRank, (int)pairRank];
        }
        public void SetActionForPair(Card.Ranks upcardRank, Card.Ranks pairRank, ActionToTake action)
        {
            pairsStrategy[(int)upcardRank, (int)pairRank] = action;
        }

        public ActionToTake GetActionForSoftHand(Card.Ranks upcardRank, int softRemainder)
        {
            return softStrategy[(int)upcardRank, softRemainder];
        }
        public void SetActionForSoftHand(Card.Ranks upcardRank, int softRemainder, ActionToTake action)
        {
            softStrategy[(int)upcardRank, softRemainder] = action;
        }

        public ActionToTake GetActionForHardHand(Card.Ranks upcardRank, int hardTotal)
        {
            return hardStrategy[(int)upcardRank, hardTotal];
        }
        public void SetActionForHardHand(Card.Ranks upcardRank, int hardTotal, ActionToTake action)
        {
            hardStrategy[(int)upcardRank, hardTotal] = action;
        }


        // the final result which we use when testing
        public ActionToTake GetActionForHand(Hand hand, Card dealerUpcard)
        {
            if (hand.HandValue() >= 21) return ActionToTake.Stand;

            var upcardRank = CollapsedRank(dealerUpcard.Rank);

            if (hand.IsPair())
            {
                var pairRank = CollapsedRank(hand.Cards[0].Rank);
                return pairsStrategy[upcardRank, pairRank];
            }

            if (hand.HasSoftAce())
            {
                // we want the total other than the high ace
                int howManyAces = hand.Cards.Count(c => c.Rank == Card.Ranks.Ace);
                int total = hand.Cards
                    .Where(c => c.Rank != Card.Ranks.Ace)
                    .Sum(c => c.RankValueHigh) +
                    (howManyAces - 1);

                return softStrategy[upcardRank, total];
            }

            return hardStrategy[upcardRank, hand.HandValue()];
        }

        private int CollapsedRank(Card.Ranks rank)
        {
            // we collapse certain ranks together because they're the same value
            switch (rank)
            {
                case Card.Ranks.Jack:
                case Card.Ranks.Queen:
                case Card.Ranks.King:
                    return (int)Card.Ranks.Ten;
            }
            return (int)rank;
        }
    }
}
