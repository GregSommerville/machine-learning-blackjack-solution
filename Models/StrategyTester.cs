using System;
using System.Collections.Generic;
using System.Linq;

namespace BlackjackStrategy.Models
{
    public enum GameState
    {
        PlayerBlackjack,
        PlayerDrawing,
        PlayerBusted,
        DealerDrawing,
        DealerBusted,
        HandComparison,
        RestartPlayerHand
    }

    class StrategyTester
    {
        public bool StackTheDeck { get; set; }

        private StrategyBase strategy;
        private TestConditions testConditions;
        
        public StrategyTester(StrategyBase strategy, TestConditions conditions)
        {
            this.strategy = strategy;
            this.testConditions = conditions;
        }

        public int GetStrategyScore(int numHandsToPlay)
        {
            int playerChips = 0;
            MultiDeck deck = new MultiDeck(testConditions.NumDecks);

            for (int handNum = 0; handNum < numHandsToPlay; handNum++)
            {
                Hand dealerHand = new Hand();
                Hand playerHand = new Hand();

                dealerHand.AddCard(deck.DealCard());
                dealerHand.AddCard(deck.DealCard());

                playerHand.AddCard(deck.DealCard());
                if (StackTheDeck)
                {
                    // even out the hands dealt to the player so it's proportionate to the 
                    // number of cells in the three grids
                    var rand = Randomizer.GetFloatFromZeroToOne();
                    if (rand < 0.33F)
                    {
                        // deal a pair
                        deck.ForceNextCardToBe(playerHand.Cards[0].Rank);
                    }
                    if (rand >= 0.33F && rand < 0.66F)
                    {
                        // deal a soft hand
                        if (playerHand.Cards[0].Rank != Card.Ranks.Ace)
                            deck.ForceNextCardToBe(Card.Ranks.Ace);
                        else
                            deck.EnsureNextCardIsnt(Card.Ranks.Ace);    // avoid a pair of Aces
                    }
                    // yes, our normal deal for hard hands may result in a pair or a hard hand, but 
                    // we don't care since we're just trying to even out the proportion of those type of hands
                }
                playerHand.AddCard(deck.DealCard());

                List<Hand> playerHands = new List<Hand>();
                playerHands.Add(playerHand);

                // we need to track how much bet per hand, since you can double down after a split.
                List<int> betAmountPerHand = new List<int>();
                betAmountPerHand.Add(testConditions.BetSize);
                playerChips -= testConditions.BetSize;

                //  1. check for player Blackjack
                if (playerHand.HandValue() == 21)
                {
                    // if the dealer also has 21, then it's a tie
                    if (dealerHand.HandValue() != 21)
                    {
                        playerChips += betAmountPerHand[0]; // return the bet
                    }
                    else
                    {
                        // Blackjack typically pays 3:2, although some casinos do 5:4
                        playerChips += testConditions.BlackjackPayoffSize;
                    }
                    betAmountPerHand[0] = 0;
                    continue;   // go to next hand
                }

                //  2. if the dealer has blackjack, then simply move to the next hand, since playerChips was already decremented
                if (dealerHand.HandValue() == 21) continue;
                
                //  3.  If the player has a playable hand, get a decision and play until standing or busting
                //      If split is selected, a new hand is added to playerHands, which is why we loop like this:
                for (var handIndex = 0; handIndex < playerHands.Count; handIndex++)
                {
                    playerHand = playerHands[handIndex];

                    var gameState = GameState.PlayerDrawing;
                    while (gameState == GameState.PlayerDrawing)
                    {
                        // if the hand was split and resulted in Blackjack, pay off and more to the next hand
                        if (playerHand.HandValue() == 21)
                        {
                            if (playerHand.Cards.Count == 2)    // Blackjack
                            {
                                int blackjackPayoff = testConditions.BlackjackPayoffSize * betAmountPerHand[handIndex] / testConditions.BetSize;
                                playerChips += blackjackPayoff;
                                betAmountPerHand[handIndex] = 0;
                            }
                            gameState = GameState.DealerDrawing;
                            break;
                         }

                        var action = strategy.GetActionForHand(playerHand, dealerHand.Cards[0]);

                        // if there's an attempt to double-down with more than 2 cards, turn into a hit
                        if (action == ActionToTake.Double && playerHand.Cards.Count > 2)
                            action = ActionToTake.Hit;

                        switch (action)
                        {
                            case ActionToTake.Hit:
                                playerHand.AddCard(deck.DealCard());

                                // if we're at 21, we automatically stand
                                if (playerHand.HandValue() == 21)
                                    gameState = GameState.DealerDrawing;

                                // did we bust?
                                if (playerHand.HandValue() > 21)
                                {
                                    betAmountPerHand[handIndex] = 0;
                                    gameState = GameState.PlayerBusted;
                                }
                                break;

                            case ActionToTake.Stand:
                                gameState = GameState.DealerDrawing;
                                break;

                            case ActionToTake.Double:
                                // double down means bet another chip, and get one and only card card
                                playerChips -= testConditions.BetSize;
                                betAmountPerHand[handIndex] += testConditions.BetSize;

                                playerHand.AddCard(deck.DealCard());
                                if (playerHand.HandValue() > 21)
                                {
                                    betAmountPerHand[handIndex] = 0;
                                    gameState = GameState.PlayerBusted;
                                }
                                else
                                    gameState = GameState.DealerDrawing;
                                break;

                            case ActionToTake.Split:
                                // add the new hand to our collection
                                var newHand = new Hand();
                                newHand.AddCard(playerHand.Cards[1]);
                                playerHand.Cards[1] = deck.DealCard();
                                newHand.AddCard(deck.DealCard());
                                playerHands.Add(newHand);

                                // our extra bet
                                playerChips -= testConditions.BetSize;  
                                betAmountPerHand.Add(testConditions.BetSize);

                                break;
                        }
                    }
                }

                // 4.  if there are playable hands for the player, get the dealer decisions
                bool playerHandsAvailable = betAmountPerHand.Sum() > 0;
                if (playerHandsAvailable)
                {
                    var gameState = GameState.DealerDrawing;

                    // draw until holding 17 or busting
                    while (dealerHand.HandValue() < 17)
                    {
                        dealerHand.AddCard(deck.DealCard());
                        if (dealerHand.HandValue() > 21)
                        {
                            // payoff each hand that is still valid - busts and blackjacks have 0 for betAmountPerHand
                            for (int handIndex = 0; handIndex < playerHands.Count; handIndex++)
                                playerChips += betAmountPerHand[handIndex] * 2;  // the original bet and a matching amount
                            gameState = GameState.DealerBusted;
                            break;
                        }
                    }

                    // 5. and then compare the dealer hand to each player hand
                    if (gameState != GameState.DealerBusted)
                    {
                        int dealerHandValue = dealerHand.HandValue();
                        for (int handIndex = 0; handIndex < playerHands.Count; handIndex++)
                        {
                            var playerHandValue = playerHands[handIndex].HandValue();

                            // if it's a tie, give the player his bet back
                            if (playerHandValue == dealerHandValue)
                            {
                                playerChips += betAmountPerHand[handIndex];
                            }
                            else
                            {
                                if (playerHandValue > dealerHandValue)
                                {
                                    // player won
                                    playerChips += betAmountPerHand[handIndex] * 2;  // the original bet and a matching amount
                                }
                                else
                                {
                                    // player lost, nothing to do since the chips have already been decremented
                                }
                            }
                        }
                    }
                }
            }

            return playerChips;
        }

        public void GetStatistics(out double average, out double stdDev, out double coeffVariation)
        {
            int numTests = testConditions.NumFinalTests;
            int totalScore = 0;
            List<int> scores = new List<int>();
            for (int i = 0; i < numTests; i++)
            {
                int score = GetStrategyScore(testConditions.NumHandsToPlay);
                totalScore += score;
                scores.Add(score);
            }

            average = totalScore / numTests;
            stdDev = StandardDeviation(scores);
            coeffVariation = stdDev / average;
        }

        private double StandardDeviation(IEnumerable<int> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

    }
}
