using System;

namespace BlackjackStrategy.Models
{
    // encapsulates one complete strategy to play Blackjack
    class Strategy : StrategyBase
    {
        public float Fitness { get; set; } = 0;

        public Strategy Clone()
        {
            var clone = new Strategy();
            clone.DeepCopy(this);
            clone.Fitness = this.Fitness;
            return clone;
        }

        public void Randomize()
        {
            foreach (var upcardRank in Card.ListOfRanks)
            {
                // randomize pairs
                foreach (var pairRank in Card.ListOfRanks)
                    SetActionForPair(upcardRank, pairRank, (ActionToTake)Randomizer.IntLessThan(NumActionsWithSplit));

                // and soft hands
                for (int softRemainder = 2; softRemainder <= HighestSoftHandRemainder; softRemainder++)
                    SetActionForSoftHand(upcardRank, softRemainder, (ActionToTake)Randomizer.IntLessThan(NumActionsNoSplit));

                // and hard hands
                for (int hardValue = 5; hardValue <= HighestHardHandValue; hardValue++)
                    SetActionForHardHand(upcardRank, hardValue, (ActionToTake)Randomizer.IntLessThan(NumActionsNoSplit));
            }
        }

        public void Mutate()
        {
            // randomly set one cell in each of the arrays

            // first the pair mutation
            var upcardRank = GetRandomRankForMutation();
            var randomPairRank = GetRandomRankForMutation();
            SetActionForPair(upcardRank, randomPairRank, (ActionToTake)Randomizer.IntLessThan(NumActionsWithSplit));

            // then soft card mutation
            upcardRank = GetRandomRankForMutation();
            var randomRemainder = Randomizer.IntBetween(LowestSoftHandRemainder, HighestSoftHandRemainder);
            SetActionForSoftHand(upcardRank, randomRemainder, (ActionToTake)Randomizer.IntLessThan(NumActionsNoSplit));

            // now hard hand
            upcardRank = GetRandomRankForMutation();
            var hardTotal = Randomizer.IntBetween(LowestHardHandValue, HighestHardHandValue);
            SetActionForHardHand(upcardRank, hardTotal, (ActionToTake)Randomizer.IntLessThan(NumActionsNoSplit));
        }

        private Card.Ranks GetRandomRankForMutation()
        {
            Card.Ranks rank;
            do
                rank = (Card.Ranks) Randomizer.IntBetween((int)Card.Ranks.Two, (int)Card.Ranks.Ace);
            while (rank == Card.Ranks.King || 
                   rank == Card.Ranks.Queen || 
                   rank == Card.Ranks.Jack);

            return rank;
        }

        public Strategy CrossOverWith(Strategy otherParent)
        {
            // here we create one child, with genetic information from each parent 
            // in proportion to their relative fitness scores
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
                    child.SetActionForPair(upcardRank, pairRank, 
                        useMyAction ?
                            this.GetActionForPair(upcardRank, pairRank) :
                            otherParent.GetActionForPair(upcardRank, pairRank));
                }

                // populate the soft hands
                for (int softRemainder = 2; softRemainder <= HighestSoftHandRemainder; softRemainder++)
                {
                    bool useMyAction = Randomizer.GetFloatFromZeroToOne() < percentageChanceOfMine;
                    child.SetActionForSoftHand(upcardRank, softRemainder, 
                        useMyAction ?
                        this.GetActionForSoftHand(upcardRank, softRemainder) :
                        otherParent.GetActionForSoftHand(upcardRank, softRemainder));
                }

                // populate the hard hands
                for (int hardValue = 5; hardValue <= HighestHardHandValue; hardValue++)
                {
                    bool useMyAction = Randomizer.GetFloatFromZeroToOne() < percentageChanceOfMine;
                    child.SetActionForHardHand(upcardRank, hardValue, 
                        useMyAction ?
                        this.GetActionForHardHand(upcardRank, hardValue) :
                        otherParent.GetActionForHardHand(upcardRank, hardValue));
                }
            }

            return child;
        }
    }
}