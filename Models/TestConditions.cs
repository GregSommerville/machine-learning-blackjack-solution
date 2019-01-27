namespace BlackjackStrategy.Models
{
    class TestConditions
    {
        public const int NumDecks = 4;
        public const int NumHandsToPlay = 50000;    // 6% chance of hitting * 25,000 hands = 1500 / 100 pair combos = 15 hands each pair/upcard 
        public const int BetSize = 2;
        public const int BlackjackPayoffSize = 3;   // if you have a blackjack, most casinos pay off 3:2

        public const int NumFinalTests = 10;
    }
}
