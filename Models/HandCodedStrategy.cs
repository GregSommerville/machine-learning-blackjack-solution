using System.Collections.Generic;

namespace BlackjackStrategy.Models
{
    // This class uses the standard Basic Strategy for Blackjack that is found in most
    // books or web resources that talk about Blackjack
    class HandCodedStrategy : StrategyBase
    {
        public void LoadStandardStrategy()
        {
            // default for everything is "Stand", so fill in the rest
            LoadHardHolding(16, "5S 5H");
            LoadHardHolding(15, "5S 5H");
            LoadHardHolding(14, "5S 5H");
            LoadHardHolding(13, "5S 5H");
            LoadHardHolding(12, "2H 3S 5H");
            LoadHardHolding(11, "10D");
            LoadHardHolding(10, "8D 2H");
            LoadHardHolding(9, "1H 4D 5H");
            LoadHardHolding(8, "10H");
            LoadHardHolding(7, "10H");
            LoadHardHolding(6, "10H");
            LoadHardHolding(5, "10H");

            LoadSoftHolding(8, "4S 1D 5S");
            LoadSoftHolding(7, "5D 2S 3H");
            LoadSoftHolding(6, "1H 4D 5H");
            LoadSoftHolding(5, "2H 3D 5H");
            LoadSoftHolding(4, "2H 3D 5H");
            LoadSoftHolding(3, "3H 2D 5H");
            LoadSoftHolding(2, "3H 2D 5H");

            LoadPairHolding(Card.Ranks.Ace, "10P");
            LoadPairHolding(Card.Ranks.Ten, "10S");
            LoadPairHolding(Card.Ranks.Nine, "5P 1S 2P 2S");
            LoadPairHolding(Card.Ranks.Eight, "10P");
            LoadPairHolding(Card.Ranks.Seven, "6P 4H");
            LoadPairHolding(Card.Ranks.Six, "5P 5H");
            LoadPairHolding(Card.Ranks.Five, "8D 2H");
            LoadPairHolding(Card.Ranks.Four, "3H 2P 5H");
            LoadPairHolding(Card.Ranks.Three, "6P 4H");
            LoadPairHolding(Card.Ranks.Two, "6P 4H");
        }

        private void LoadHardHolding(int total, string actionString)
        {
            var actions = GetActionsFromString(actionString);
            for (int i = 0; i < actions.Count; i++)
                SetActionForHardHand(i, total, actions[i]);
        }

        private void LoadSoftHolding(int softRemainder, string actionString)
        {
            var actions = GetActionsFromString(actionString);
            for (int i = 0; i < actions.Count; i++)
                SetActionForSoftHand(i, softRemainder, actions[i]);
        }

        private void LoadPairHolding(Card.Ranks pairRank, string actionString)
        {
            var actions = GetActionsFromString(actionString);
            for (int i = 0; i < actions.Count; i++)
                SetActionForPair(i, IndexFromRank(pairRank), actions[i]);
        }

        private List<ActionToTake> GetActionsFromString(string s)
        {
            var result = new List<ActionToTake>();
            var parts = s.Split(" ".ToCharArray());
            foreach (var part in parts)
            {
                string actionString = part.Substring(part.Length - 1, 1);
                int numTimes = int.Parse(part.Substring(0, part.Length - 1));
                for (int i = 0; i < numTimes; i++)
                {
                    switch (actionString)
                    {
                        case "H":
                            result.Add(ActionToTake.Hit);
                            break;

                        case "P":
                            result.Add(ActionToTake.Split);
                            break;

                        case "S":
                            result.Add(ActionToTake.Stand);
                            break;

                        case "D":
                            result.Add(ActionToTake.Double);
                            break;
                    }
                }
            }
            return result;
        }
    }
}
