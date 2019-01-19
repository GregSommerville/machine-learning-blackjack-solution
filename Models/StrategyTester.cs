using System.Collections.Generic;
using System.Diagnostics;

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
        private Strategy strategy;
        
        public StrategyTester(Strategy strategy)
        {
            this.strategy = strategy;
        }

        public int GetStrategyScore(int numHandsToPlay)
        {
            int playerChips = 0;
            MultiDeck deck = new MultiDeck(TestConditions.NumDecks);

            for (int handNum = 0; handNum < numHandsToPlay; handNum++)
            {
                Hand dealerHand = new Hand();
                Hand playerHand = new Hand();

                dealerHand.AddCard(deck.DealCard());
                dealerHand.AddCard(deck.DealCard());

                playerHand.AddCard(deck.DealCard());
                playerHand.AddCard(deck.DealCard());

                List<Hand> playerHands = new List<Hand>();
                playerHands.Add(playerHand);

                // we need to track how much bet per hand, since you can double down after a split.
                List<int> betAmountPerHand = new List<int>();
                betAmountPerHand.Add(TestConditions.BetSize);
                playerChips -= TestConditions.BetSize;

                var currentHandState = GameState.PlayerDrawing;

                // loop over each hand the player holds, which only happens when they've split a hand
                for (int handIndex = 0; handIndex < playerHands.Count; handIndex++)
                {
                    playerHand = playerHands[handIndex];

                    // check for player having a blackjack, which is an instant win
                    if (playerHand.HandValue() == 21)
                    {
                        // if the dealer also has 21, then it's a tie
                        if (dealerHand.HandValue() != 21)
                        {
                            currentHandState = GameState.PlayerBlackjack;
                            playerChips += TestConditions.BlackjackPayoffSize;
                            betAmountPerHand[handIndex] = 0;
                        }
                        else
                        {
                            // a tie means we just ignore it and drop through
                            currentHandState = GameState.HandComparison;
                        }
                    }

                    // check for dealer having blackjack, which is either instant loss or tie 
                    if (dealerHand.HandValue() == 21) currentHandState = GameState.HandComparison;

                    // player draws 
                    while (currentHandState == GameState.PlayerDrawing)
                    {
                        var action = strategy.GetActionForHand(playerHand, dealerHand.Cards[0]);

                        // if there's an attempt to double-down with more than 2 cards, turn into a hit
                        if (action == ActionToTake.Double && playerHand.Cards.Count > 2)
                            action = ActionToTake.Hit;

                        switch (action)
                        {
                            case ActionToTake.Hit:
                                playerHand.AddCard(deck.DealCard());
                                
                                // if we're at 21, we're done
                                if (playerHand.HandValue() == 21)
                                    currentHandState = GameState.DealerDrawing;

                                // did we bust?
                                if (playerHand.HandValue() > 21)
                                {
                                    currentHandState = GameState.PlayerBusted;
                                    betAmountPerHand[handIndex] = 0;
                                }
                                break;

                            case ActionToTake.Stand:
                                // if player stands, it's the dealer's turn to draw
                                currentHandState = GameState.DealerDrawing;
                                break;

                            case ActionToTake.Double:
                                // double down means bet another chip, and get one and only card card
                                playerChips -= TestConditions.BetSize;
                                betAmountPerHand[handIndex] += TestConditions.BetSize;

                                playerHand.AddCard(deck.DealCard());
                                if (playerHand.HandValue() > 21)
                                {
                                    currentHandState = GameState.PlayerBusted;
                                    betAmountPerHand[handIndex] = 0;
                                }
                                else
                                    currentHandState = GameState.DealerDrawing;
                                break;

                            case ActionToTake.Split:

                                // do the split and add the hand to our collection
                                var newHand = new Hand();
                                newHand.AddCard(playerHand.Cards[1]);
                                playerHand.Cards[1] = deck.DealCard();
                                newHand.AddCard(deck.DealCard());
                                playerHands.Add(newHand);

                                // our extra bet
                                playerChips -= TestConditions.BetSize;  // no need to adjust totalBetAmount 
                                betAmountPerHand.Add(TestConditions.BetSize);

                                // is the hand now 21?
                                if (playerHand.HandValue() == 21)
                                {
                                    if (dealerHand.HandValue() != 21)
                                    {
                                        currentHandState = GameState.PlayerBlackjack;
                                        playerChips += TestConditions.BlackjackPayoffSize;
                                        betAmountPerHand[handIndex] = 0;    
                                    }
                                    else
                                    {
                                        // a tie means we just ignore it and drop through
                                        currentHandState = GameState.HandComparison;
                                    }
                                }

                                break;
                        }
                    }
                }

                while (currentHandState == GameState.DealerDrawing)
                {
                    // if player didn't bust or blackjack, dealer hits until they have 17+ (stands on soft 17)
                    if (dealerHand.HandValue() < 17)
                    {
                        dealerHand.AddCard(deck.DealCard());
                        if (dealerHand.HandValue() > 21)
                        {
                            currentHandState = GameState.DealerBusted;

                            // payoff each hand that is still valid - busts and blackjacks have 0 for betAmountPerHand
                            for (int handIndex = 0; handIndex < playerHands.Count; handIndex++)
                                    playerChips += betAmountPerHand[handIndex] * 2;  // the original bet and a matching amount
                        }
                    }
                    else
                    {
                        // dealer hand is 17+, so we're done
                        currentHandState = GameState.HandComparison;
                    }
                }

                if (currentHandState == GameState.HandComparison)
                {
                    int dealerHandValue = dealerHand.HandValue();

                    // compare each hand that is still valid
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

            return playerChips;
        }
    }
}
