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
                // for each hand, we generate a random deck.  Blackjack is often played with multiple decks to improve the house edge
                Hand dealerHand = new Hand();
                Hand playerHand = new Hand();

                dealerHand.AddCard(deck.DealCard());
                dealerHand.AddCard(deck.DealCard());

                playerHand.AddCard(deck.DealCard());
                playerHand.AddCard(deck.DealCard());

                // save the cards in state, and reset the votes for this hand
                List<Hand> playerHands = new List<Hand>();
                playerHands.Add(playerHand);

                // do the intial wager
                playerChips -= TestConditions.BetSize;

                // outer loop is for each hand the player holds.  This only happens when they've split a hand
                for (int handIndex = 0; handIndex < playerHands.Count; handIndex++)
                {
                    playerHand = playerHands[handIndex];
                    int totalBetAmount = TestConditions.BetSize;

                    // loop until the hand is done
                    var currentHandState = GameState.PlayerDrawing;

                    // check for player having a blackjack, which is an instant win
                    if (playerHand.HandValue() == 21)
                    {
                        // if the dealer also has 21, then it's a tie
                        if (dealerHand.HandValue() != 21)
                        {
                            currentHandState = GameState.PlayerBlackjack;
                            playerChips += TestConditions.BlackjackPayoffSize;
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
                                // hit me
                                playerHand.AddCard(deck.DealCard());
                                // if we're at 21, we're done
                                if (playerHand.HandValue() == 21)
                                    currentHandState = GameState.DealerDrawing;
                                // did we bust?
                                if (playerHand.HandValue() > 21)
                                    currentHandState = GameState.PlayerBusted;
                                break;

                            case ActionToTake.Stand:
                                // if player stands, it's the dealer's turn to draw
                                currentHandState = GameState.DealerDrawing;
                                break;

                            case ActionToTake.Double:
                                // double down means bet another chip, and get one and only card card
                                playerChips -= TestConditions.BetSize;
                                totalBetAmount += TestConditions.BetSize;
                                playerHand.AddCard(deck.DealCard());
                                if (playerHand.HandValue() > 21)
                                    currentHandState = GameState.PlayerBusted;
                                else
                                    currentHandState = GameState.DealerDrawing;
                                break;

                            case ActionToTake.Split:
                                Debug.Assert(playerHand.IsPair(), "Split with non-pair!");

                                // do the split and add the hand to our collection
                                var newHand = new Hand();
                                newHand.AddCard(playerHand.Cards[1]);
                                playerHand.Cards[1] = deck.DealCard();
                                newHand.AddCard(deck.DealCard());
                                playerHands.Add(newHand);

                                // 16 because there are 4 of each rank * 4 decks = 16 possible hands if you split all of the Aces, for example
                                Debug.Assert(playerHands.Count < 16, "Too many hands");

                                // our extra bet
                                playerChips -= TestConditions.BetSize;  // no need to adjust totalBetAmount 

                                // is the new hand now 21?, if so, we're done
                                if (playerHand.HandValue() == 21)
                                    currentHandState = GameState.DealerDrawing;

                                // did we bust?
                                if (playerHand.HandValue() > 21)
                                    currentHandState = GameState.PlayerBusted;

                                break;
                        }
                    }

                    // if the player busted, nothing to do, since chips have already been consumed.  Just go on to the next hand
                    // on the other hand, if the player hasn't busted, then we need to play the hand for the dealer
                    while (currentHandState == GameState.DealerDrawing)
                    {
                        // if player didn't bust or blackjack, dealer hits until they have 17+ (hits on soft 17)
                        if (dealerHand.HandValue() < 17)
                        {
                            dealerHand.AddCard(deck.DealCard());
                            if (dealerHand.HandValue() > 21)
                            {
                                currentHandState = GameState.DealerBusted;
                                playerChips += totalBetAmount * 2;  // the original bet and a matching amount
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
                        int playerHandValue = playerHand.HandValue();
                        int dealerHandValue = dealerHand.HandValue();

                        // if it's a tie, give the player his bet back
                        if (playerHandValue == dealerHandValue)
                        {
                            playerChips += totalBetAmount;
                        }
                        else
                        {
                            if (playerHandValue > dealerHandValue)
                            {
                                // player won
                                playerChips += totalBetAmount * 2;  // the original bet and a matching amount
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
