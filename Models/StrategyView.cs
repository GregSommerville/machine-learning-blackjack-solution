using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BlackjackStrategy.Models
{
    class StrategyView
    {
        public static void ShowPlayableHands(StrategyBase strategy, Canvas canvas, string savedImageName, string displayText)
        {
            // clear the screen
            canvas.Children.Clear();

            Color   hitColor = Colors.LightGreen, 
                    standColor = Color.FromRgb(252, 44, 44),
                    doubleColor = Colors.Yellow, 
                    splitColor = Colors.MediumPurple;

            AddInformationalText(displayText, canvas);

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

                    playerHand.AddCard(new Card((Card.Ranks)firstCardRank, Card.Suits.Diamonds));
                    playerHand.AddCard(new Card((Card.Ranks)secondCardRank, Card.Suits.Hearts));

                    // get strategy and display
                    var action = strategy.GetActionForHand(playerHand, dealerUpcard);
                    switch (action)
                    {
                        case ActionToTake.Hit:
                            AddColorBox(hitColor, "H", x, y, canvas);
                            break;

                        case ActionToTake.Stand:
                            AddColorBox(standColor, "S", x, y, canvas);
                            break;

                        case ActionToTake.Double:
                            AddColorBox(doubleColor, "D", x, y, canvas);
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
                            AddColorBox(hitColor, "H", x, y, canvas);
                            break;

                        case ActionToTake.Stand:
                            AddColorBox(standColor, "S", x, y, canvas);
                            break;

                        case ActionToTake.Double:
                            AddColorBox(doubleColor, "D", x, y, canvas);
                            break;
                    }
                    y++;
                }
                x++;
            }

            // finally, a grid for pairs
            int startY = y + 1;
            AddColorBox(Colors.White, "", leftColumnForAces, startY, canvas);
            x = leftColumnForAces + 1;
            foreach (var upcardRank in Card.ListOfRanks)
            {
                // strategy for 10, J, Q, and K are the same, so skip some of those 
                if (upcardRank == Card.Ranks.Jack || upcardRank == Card.Ranks.Queen || upcardRank == Card.Ranks.King)
                    continue;

                string upcardRankName = Card.RankText(upcardRank);
                Card dealerUpcard = new Card(upcardRank, Card.Suits.Diamonds);

                AddColorBox(Colors.White, upcardRankName, x, startY, canvas);
                y = startY + 1;

                for (var pairedRank = Card.Ranks.Ace; pairedRank >= Card.Ranks.Two; pairedRank--)
                {
                    // strategy for 10, J, Q, and K are the same, so skip some of those 
                    if (pairedRank == Card.Ranks.Jack || pairedRank == Card.Ranks.Queen || pairedRank == Card.Ranks.King)
                        continue;

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
                            AddColorBox(hitColor, "H", x, y, canvas);
                            break;

                        case ActionToTake.Stand:
                            AddColorBox(standColor, "S", x, y, canvas);
                            break;

                        case ActionToTake.Double:
                            AddColorBox(doubleColor, "D", x, y, canvas);
                            break;

                        case ActionToTake.Split:
                            AddColorBox(splitColor, "P", x, y, canvas);
                            break;
                    }
                    y++;
                }
                x++;
            }

            // now that's it's drawn, save an image?
            if (!string.IsNullOrEmpty(savedImageName))
                SaveCanvasToPng(canvas, savedImageName);
        }

        private static void AddColorBox(Color color, string label, int x, int y, Canvas canvas)
        {
            int columnWidth = (int) canvas.ActualWidth / 25; 
            int rowHeight = (columnWidth * 4) / 5;
            int startX = columnWidth;
            int startY = columnWidth;

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

        private static void AddInformationalText(string message, Canvas canvas)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            var itemText = new TextBlock();
            itemText.HorizontalAlignment = HorizontalAlignment.Left;
            itemText.VerticalAlignment = VerticalAlignment.Center;
            itemText.Inlines.Add(new Bold(new Run(message)));
            itemText.FontSize = 20;

            int spacing = (int)canvas.ActualWidth / 25;
            int bottomY = (int)canvas.ActualHeight;
            canvas.Children.Add(itemText);
            Canvas.SetTop(itemText, bottomY - spacing * 1.5);
            Canvas.SetLeft(itemText, spacing);
        }

        private static void SaveCanvasToPng(Canvas canvas, string savedImageName)
        {
            Size size = canvas.RenderSize;
            Rect rect = new Rect(size);

            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)rect.Right,
                (int)rect.Bottom, 
                96d, 96d, 
                PixelFormats.Default);

            // make sure all of the children show up
            canvas.Measure(size);
            canvas.Arrange(rect);
            rtb.Render(canvas);

            //endcode as PNG
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            //save to memory stream
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            pngEncoder.Save(ms);
            ms.Close();
            System.IO.File.WriteAllBytes(savedImageName + ".png", ms.ToArray());
        }
    }
}
