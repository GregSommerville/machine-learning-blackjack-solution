using System.ComponentModel;

namespace BlackjackStrategy.Models
{
    [DisplayName("Test Settings")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TestConditions 
    {
        [Description("Number of decks")]
        public int NumDecks { get; set; } = 4;

        [Description("Number of hands each candidate is tested with")]
        public int NumHandsToPlay { get; set; } = 100000; 

        [Description("Bet size")]
        public int BetSize { get; set; } = 2;

        [Description("Payoff amount for player Blackjack")]
        public int BlackjackPayoffSize { get; set; } = 3;   // if you have a blackjack, most casinos pay off 3:2

        [Description("For the post-generation summary, how many tests are run")]
        public int NumFinalTests  { get; set; }= 100;

        [Description("Gives the player better hands, resulting in a more even filling in of the three grids")]
        public bool StackTheDeck { get; set; } = false;

        [Description("If checked, a PNG of each generation's best will be saved to the program's current folder")]
        public bool SaveImagePerGeneration { get; set; } = false;

        // so it looks right in the property grid
        public override string ToString()
        {
            return "";
        }
    }
}
