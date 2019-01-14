using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlackjackStrategy.Models
{
    class StrategyView
    {
        public static void ShowPlayableHands(Strategy strategy, Canvas canvas)
        {
            // clear the screen
            canvas.Children.Clear();

            // display a grid for non-paired hands without an ace.  One column for each possible dealer upcard
            AddColorBox(Colors.White, "", 0, 0, canvas);
            int x = 1, y = 0;
            foreach (var upcardRank in Card.ListOfRanks)
            {
                // strategy for 10, J, Q, and K are the same, so skip some of those 
                if (upcardRank == Card.Ranks.Jack || upcardRank == Card.Ranks.Queen || upcardRank == Card.Ranks.King)
                    continue;
                
                Card dealerUpcard = new Card(upcardRank, Card.Suits.Diamonds);

                string upcardRankName = Card.RankText(upcardRank);
                AddColorBox(Colors.White, upcardRankName, x, 0, canvas);
                y = 1;

                for (int hardTotal = 20; hardTotal > 4; hardTotal--)
                {
                    // add a white box with the total
                    AddColorBox(Colors.White, hardTotal.ToString(), 0, y, canvas);

                    // build player hand
                    Hand playerHand = new Hand();

                    // divide by 2 if it's even, else add one and divide by two
                    int firstCardRank = ((hardTotal % 2) != 0) ? (hardTotal + 1) / 2 : hardTotal / 2;
                    int secondCardRank = hardTotal - firstCardRank;
                    if (firstCardRank == secondCardRank)
                    {
                        firstCardRank++;
                        secondCardRank--;

                        if (firstCardRank == 11)
                        {
                            // hard 20 needs to be three cards, so in this case 9, 4, 7
                            firstCardRank = 4;
                            playerHand.AddCard(new Card(Card.Ranks.Seven, Card.Suits.Clubs));
                        }
                    }

                    playerHand.AddCard(new Card(firstCardRank, Card.Suits.Diamonds));
                    playerHand.AddCard(new Card(secondCardRank, Card.Suits.Hearts));

                    // get strategy and display
                    var action = strategy.GetActionForHand(playerHand, dealerUpcard);
                    switch (action)
                    {
                        case ActionToTake.Hit:
                            AddColorBox(Colors.Green, "H", x, y, canvas);
                            break;

                        case ActionToTake.Stand:
                            AddColorBox(Colors.Red, "S", x, y, canvas);
                            break;

                        case ActionToTake.Double:
                            AddColorBox(Colors.Yellow, "D", x, y, canvas);
                            break;
                    }
                    y++;
                }
                x++;
            }

            // and another for hands with an ace
            // display a grid for hands without an ace.  One column for each possible dealer upcard
            const int leftColumnForAces = 12;
            AddColorBox(Colors.White, "", leftColumnForAces, 0, canvas);
            x = leftColumnForAces + 1;
            foreach (var upcardRank in Card.ListOfRanks)
            {
                // strategy for 10, J, Q, and K are the same, so skip some of those 
                if (upcardRank == Card.Ranks.Jack || upcardRank == Card.Ranks.Queen || upcardRank == Card.Ranks.King)
                    continue;

                string upcardRankName = Card.RankText(upcardRank);
                Card dealerUpcard = new Card(upcardRank, Card.Suits.Diamonds);

                AddColorBox(Colors.White, upcardRankName, x, 0, canvas);
                y = 1;

                // we don't start with Ace, because that would be AA, which is handled in the pair zone
                // we also don't start with 10, since that's blackjack.  So 9 is our starting point
                // only need 2 through 9, since a-a is in the pairs table, and a-10 is blackjack
                for (var otherCard = Card.Ranks.Nine; otherCard >= Card.Ranks.Two; otherCard--)
                {
                    // add a white box with the player hand: "A-x"
                    string otherCardRank = Card.RankText(otherCard);
                    AddColorBox(Colors.White, "A-" + otherCardRank, leftColumnForAces, y, canvas);

                    // build player hand
                    Hand playerHand = new Hand();
                    // first card is an ace, second card is looped over
                    playerHand.AddCard(new Card(Card.Ranks.Ace, Card.Suits.Hearts)); // ace of hearts
                    playerHand.AddCard(new Card(otherCard, Card.Suits.Spades));

                    // get strategy and display
                    var action = strategy.GetActionForHand(playerHand, dealerUpcard);
                    switch (action)
                    {
                        case ActionToTake.Hit:
                            AddColorBox(Colors.Green, "H", x, y, canvas);
                            break;

                        case ActionToTake.Stand:
                            AddColorBox(Colors.Red, "S", x, y, canvas);
                            break;

                        case ActionToTake.Double:
                            AddColorBox(Colors.Yellow, "D", x, y, canvas);
                            break;
                    }
                    y++;
                }
                x++;
            }

            // finally, a grid for pairs
            int startY = y + 1;
            AddColorBox(Colors.White, "", leftColumnForAces, 0, canvas);
            x = leftColumnForAces + 1;
            foreach (var upcardRank in Card.ListOfRanks)
            {
                // strategy for 10, J, Q, and K are the same, so skip some of those 
                if (upcardRank == Card.Ranks.Jack || upcardRank == Card.Ranks.Queen || upcardRank == Card.Ranks.King)
                    continue;

                string upcardRankName = Card.RankText(upcardRank);
                Card dealerUpcard = new Card(upcardRank, Card.Suits.Diamonds);

                AddColorBox(Colors.White, upcardRankName, x, 0, canvas);
                y = startY;

                foreach (var pairedRank in Card.ListOfRanks)
                {
                    // add a white box with the player hand: "x-x"
                    string pairedCardRank = Card.RankText(pairedRank);
                    AddColorBox(Colors.White, pairedCardRank + "-" + pairedCardRank, leftColumnForAces, y, canvas);

                    // build player hand
                    Hand playerHand = new Hand();
                    playerHand.AddCard(new Card(pairedRank, Card.Suits.Hearts)); // X of hearts
                    playerHand.AddCard(new Card(pairedRank, Card.Suits.Spades)); // X of spades

                    // get strategy and display
                    var action = strategy.GetActionForHand(playerHand, dealerUpcard);
                    switch (action)
                    {
                        case ActionToTake.Hit:
                            AddColorBox(Colors.Green, "H", x, y, canvas);
                            break;

                        case ActionToTake.Stand:
                            AddColorBox(Colors.Red, "S", x, y, canvas);
                            break;

                        case ActionToTake.Double:
                            AddColorBox(Colors.Yellow, "D", x, y, canvas);
                            break;

                        case ActionToTake.Split:
                            AddColorBox(Colors.LightBlue, "P", x, y, canvas);
                            break;
                    }
                    y++;
                }
                x++;
            }
        }

        private static void AddColorBox(Color color, string label, int x, int y, Canvas canvas)
        {
            // easy to do constants when the screen isn't meant to resize
            const int
                columnWidth = 38,
                rowHeight = 28,
                startX = 20,
                startY = 20;

            // the element is a border
            var box = new Border();
            box.BorderBrush = Brushes.Black;
            box.BorderThickness = new System.Windows.Thickness(1);
            box.Background = new SolidColorBrush(color);
            box.Width = columnWidth;
            box.Height = rowHeight;

            // and a label as a child
            var itemText = new TextBlock();
            itemText.HorizontalAlignment = HorizontalAlignment.Center;
            itemText.VerticalAlignment = VerticalAlignment.Center;
            itemText.Text = label;
            box.Child = itemText;

            canvas.Children.Add(box);
            Canvas.SetTop(box, startY + y * rowHeight);
            Canvas.SetLeft(box, startX + x * columnWidth);
        }

    }
}
