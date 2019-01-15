using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlackjackStrategy.Models
{
    public enum ActionToTake { Stand, Hit, Double, Split };

    // encapsulates one complete strategy to play Blackjack
    class Strategy
    {
        public float Fitness { get; set; } = 0;

        // we store the strategies as three multidimensional arrays, where a few spots in each array won't be used.
        // this is quicker than adjusting the index for each reference, like this: pairsStrategy[currentUpcardRank - baseRank, currentPairRank - baseRank]
        public static int HighestUpcardRank = (int)Card.Ranks.Ace;

        public static int HighestPairRank = (int)Card.Ranks.Ace;

        // soft remainders go from 2 to 9, since A-10 is blackjack, and A-1 isn't possible (would be A-A)
        public static int LowestSoftHandRemainder = 2;
        public static int HighestSoftHandRemainder = 9;

        // hard hand values go from 5 (since 4 is 2-2) to 20 (since 21 is Blackjack)
        public static int LowestHardHandValue = 5;
        public static int HighestHardHandValue = 20;

        private ActionToTake[,] pairsStrategy, softStrategy, hardStrategy;
        private const int numActionsNoSplit = 3; // Stand, Hit, Double
        private const int numActionsWithSplit = 4;  // Stand, Hit, Double, Split


        public Strategy()
        {
            pairsStrategy = new ActionToTake[HighestUpcardRank + 1, HighestPairRank + 1];
            softStrategy = new ActionToTake[HighestUpcardRank + 1, HighestSoftHandRemainder + 1];
            hardStrategy = new ActionToTake[HighestUpcardRank + 1, HighestHardHandValue + 1];
        }

        // copy constructor
        public Strategy(ActionToTake[,] pairsStrategy, ActionToTake[,] softStrategy, ActionToTake[,] hardStrategy, float fitness)
        {
            this.pairsStrategy = (ActionToTake[,])pairsStrategy.Clone();
            this.softStrategy = (ActionToTake[,])softStrategy.Clone();
            this.hardStrategy = (ActionToTake[,])hardStrategy.Clone();
            this.Fitness = fitness;
        }

        public Strategy Clone()
        {
            return new Strategy(this.pairsStrategy, this.softStrategy, this.hardStrategy, this.Fitness);
        }

        public void Randomize()
        {
            foreach (var upcardRank in Card.ListOfRanks)
            {
                // randomize pairs
                foreach (var pairRank in Card.ListOfRanks)
                    pairsStrategy[(int)upcardRank, (int)pairRank] = (ActionToTake)Randomizer.IntLessThan(numActionsWithSplit);

                // and soft hands
                for (int softRemainder = 2; softRemainder <= HighestSoftHandRemainder; softRemainder++)
                    softStrategy[(int)upcardRank, softRemainder] = (ActionToTake)Randomizer.IntLessThan(numActionsNoSplit);

                // and hard hands
                for (int hardValue = 5; hardValue <= HighestHardHandValue; hardValue++)
                    hardStrategy[(int)upcardRank, hardValue] = (ActionToTake)Randomizer.IntLessThan(numActionsNoSplit);
            }
        }

        public void Mutate()
        {
            // if selected to mutate, randomly set one of each of the arrays

            // first the pair mutation
            var upcardRank = Randomizer.IntBetween((int)Card.Ranks.Two, (int)Card.Ranks.Ace);
            var randomPairRank = Randomizer.IntBetween((int)Card.Ranks.Two, (int)Card.Ranks.Ace);
            pairsStrategy[upcardRank, randomPairRank] = (ActionToTake)Randomizer.IntLessThan(numActionsWithSplit);

            // then soft card mutation
            upcardRank = Randomizer.IntBetween((int)Card.Ranks.Two, (int)Card.Ranks.Ace);
            var randomRemainder = Randomizer.IntBetween(LowestSoftHandRemainder, HighestSoftHandRemainder);
            softStrategy[upcardRank, randomRemainder] = (ActionToTake)Randomizer.IntLessThan(numActionsNoSplit);

            // now hard hand
            upcardRank = Randomizer.IntBetween((int)Card.Ranks.Two, (int)Card.Ranks.Ace);
            var hardTotal = Randomizer.IntBetween(LowestHardHandValue, HighestHardHandValue);
            hardStrategy[upcardRank, hardTotal] = (ActionToTake)Randomizer.IntLessThan(numActionsNoSplit);
        }

        public Strategy CrossOverWith(Strategy otherParent)
        {
            // here we create one child, with genetic information from each parent in proportion to their
            // relative fitness scores
            float myScore = this.Fitness;
            float theirScore = otherParent.Fitness;
            float percentageChanceOfMine = 0;

            // it depends on whether the numbers are positive or negative
            if (myScore >= 0 && theirScore >= 0)
            {
                float totalScore = (myScore + theirScore);
                // safety check (avoiding / 0)
                if (totalScore < 0.001) totalScore = 1;
                percentageChanceOfMine = (myScore / totalScore);
            }
            else if (myScore >= 0 && theirScore < 0)
            {
                // hard to compare a positive and a negative, so let's tip the hat to Mr. Pareto
                percentageChanceOfMine = 0.8F;
            }
            else if (myScore < 0 && theirScore >= 0)
            {
                // hard to compare a positive and a negative, so let's tip the hat to Mr. Pareto
                percentageChanceOfMine = 0.2F;
            }
            else
            {
                // both negative, so use abs value and 1-(x)
                myScore = Math.Abs(myScore);
                theirScore = Math.Abs(theirScore);
                percentageChanceOfMine = 1 - (myScore / (myScore + theirScore));
            }

            // and make sure we get some kind of crossover, so clamp values between 0.2 and 0.8
            if (percentageChanceOfMine > 0.8F) percentageChanceOfMine = 0.8F;
            if (percentageChanceOfMine < 0.2F) percentageChanceOfMine = 0.2F;

            var child = new Strategy();
            foreach (var upcardRank in Card.ListOfRanks)
            {
                // populate the pairs
                foreach (var pairRank in Card.ListOfRanks)
                {
                    bool useMyAction = Randomizer.GetFloatFromZeroToOne() < percentageChanceOfMine;
                    child.SetActionForPair(upcardRank, pairRank, useMyAction ?
                            this.GetActionForPair(upcardRank, pairRank) :
                            otherParent.GetActionForPair(upcardRank, pairRank));
                }

                // populate the soft hands
                for (int softRemainder = 2; softRemainder <= HighestSoftHandRemainder; softRemainder++)
                {
                    bool useMyAction = Randomizer.GetFloatFromZeroToOne() < percentageChanceOfMine;
                    child.SetActionForSoftHand(upcardRank, softRemainder, useMyAction ?
                        this.GetActionForSoftHand(upcardRank, softRemainder) :
                        otherParent.GetActionForSoftHand(upcardRank, softRemainder));
                }

                // populate the hard hands
                for (int hardValue = 5; hardValue <= HighestHardHandValue; hardValue++)
                {
                    bool useMyAction = Randomizer.GetFloatFromZeroToOne() < percentageChanceOfMine;
                    child.SetActionForHardHand(upcardRank, hardValue, useMyAction ?
                        this.GetActionForHardHand(upcardRank, hardValue) :
                        otherParent.GetActionForHardHand(upcardRank, hardValue));
                }
            }

            return child;
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

            // we collapse certain ranks together
            var upcardRank = dealerUpcard.Rank;
            if (upcardRank == Card.Ranks.Jack || upcardRank == Card.Ranks.Queen || upcardRank == Card.Ranks.King)
                upcardRank = Card.Ranks.Ten;

            if (hand.IsPair())
            {
                var pairRank = hand.Cards[0].Rank;
                if (pairRank == Card.Ranks.Jack || pairRank == Card.Ranks.Queen || pairRank == Card.Ranks.King)
                    pairRank = Card.Ranks.Ten;
                return pairsStrategy[(int)upcardRank, (int)pairRank];
            }

            if (hand.HasSoftAce())
            {
                // we want the total other than the high ace
                int howManyAces = hand.Cards.Count(c => c.Rank == Card.Ranks.Ace);
                int total = hand.Cards
                    .Where(c => c.Rank != Card.Ranks.Ace)
                    .Sum(c => c.RankValueHigh) + 
                    (howManyAces - 1);

                return softStrategy[(int)upcardRank, total];
            }

            return hardStrategy[(int)upcardRank, hand.HandValue()];
        }
    }
}
