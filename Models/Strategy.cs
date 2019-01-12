using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlackjackStrategy.Models
{
    enum ActionToTake { Stand, Hit, Double, Split };

    // encapsulates one complete strategy to play Blackjack
    class Strategy
    {
        private ActionToTake[,] pairsStrategy, softStrategy, hardStrategy;

        public Strategy()
        {
            // this constructor creates a random representation
        }

        //-------------------------------------------------------------------------------------------

        public void AddPairStrategy(string pairRank, ActionToTake action, Card dealerUpcard)
        {
//            pairsStrategy[CleanRank(pairRank) + CleanRank(dealerUpcard.Rank)] = action;
        }

        public void AddSoftStrategy(string secondaryCardRank, ActionToTake action, Card dealerUpcard)
        {
            // secondary rank is the non-Ace
  //          softStrategy[CleanRank(secondaryCardRank) + CleanRank(dealerUpcard.Rank)] = action;
        }

        public void AddHardStrategy(int handTotal, ActionToTake action, Card dealerUpcard)
        {
            // handTotal goes from 5 (since a total of 4 means a pair of 2s) to 20
    //        hardStrategy[handTotal + CleanRank(dealerUpcard.Rank)] = action;
        }

        private string CleanRank(string rank)
        {
            if (rank == "10" || rank == "J" || rank == "Q" || rank == "K")
                rank = "T";  // we only stored one value for the tens
            return rank;
        }

        //-------------------------------------------------------------------------------------------

        public ActionToTake GetActionForHand(Hand hand, Card dealerUpcard)
        {
            if (hand.HandValue() >= 21) return ActionToTake.Stand;

            string upcardRank = CleanRank(dealerUpcard.Rank);

            if (hand.IsPair())
            {
                string rank = CleanRank(hand.Cards[0].Rank);
                return pairsStrategy[rank + upcardRank];
            }

            if (hand.HasSoftAce())
            {
                // we want the total other than the high ace
                int howManyAces = hand.Cards.Count(c => c.Rank == "A");
                int total = hand.Cards
                    .Where(c => c.Rank != "A")
                    .Sum(c => c.RankValueHigh) + 
                    (howManyAces - 1);

                string rank = (total == 10) ? "T" : total.ToString();
                return softStrategy[rank + upcardRank];
            }

            return hardStrategy[hand.HandValue() + upcardRank];
        }
    }
}
