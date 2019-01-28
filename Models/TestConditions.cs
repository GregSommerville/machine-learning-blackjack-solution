using System.ComponentModel;

namespace BlackjackStrategy.Models
{
    [DisplayName("Test Settings")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TestConditions 
    {
        public int NumDecks { get; set; } = 4;
        public int NumHandsToPlay { get; set; } = 25000;    // 6% chance of hitting * 25,000 hands = 1500 / 100 pair combos = 15 hands each pair/upcard 
        public int BetSize { get; set; } = 2;
        public int BlackjackPayoffSize { get; set; } = 3;   // if you have a blackjack, most casinos pay off 3:2
        public int NumFinalTests  { get; set; }= 10;
        public bool UseBalancedDeck { get; set; } = true;
        public bool SaveImgPerGen { get; set; } = false;
    }
}
