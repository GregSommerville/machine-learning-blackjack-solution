using BlackjackStrategy.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BlackjackStrategy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // This parameters object is bound to the UI, for editing
        public EngineParameters EngineParams { get; set; } = new EngineParameters();

        // each callback adds a progress string here 
        private List<string> progressSoFar = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            gaResultTB.Text = "Creating solution, please wait...";

            // Finding the solution takes a while, so kick off a thread for it
            Task.Factory.StartNew(() => AsyncCall());
        }

        private void AsyncCall()
        {
            // reset the progress messages
            progressSoFar = new List<string>();

            // find the solution


            // then display the final results
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var strategy = new Strategy("");
                ShowPlayableHands(strategy);

                // test it 
                string scoreResults = "";
                var tester = new StrategyTester(strategy);
                int totalScore = 0;
                for (int i = 0; i < TestConditions.NumFinalTests; i++)
                {
                    int score = tester.GetStrategyScore(TestConditions.NumHandsToPlay);
                    totalScore += score;
                    scoreResults += score + "\n";
                }
                scoreResults += "\nAverage score: " + (totalScore / TestConditions.NumFinalTests).ToString("0");

                gaResultTB.Text = "Solution found.\nScores:\n" + scoreResults;
            }),
            DispatcherPriority.Background);
        }

        private void DisplayCurrentStatus(string status)
        {
            progressSoFar.Insert(0, status);
            string allStatuses = String.Join("\n", progressSoFar);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                gaResultTB.Text = allStatuses;
            }),
            DispatcherPriority.Background);
        }

        private void ShowPlayableHands(Strategy strategy)
        {
            // clear the screen
            canvas.Children.Clear();

            // display a grid for non-paired hands without an ace.  One column for each possible dealer upcard
            AddColorBox(Colors.White, "", 0, 0);
            int x = 1, y = 0;
            for (int upcardRank = 2; upcardRank < 12; upcardRank++)
            {
                string upcardRankName = (upcardRank == 11) ? "A" : upcardRank.ToString();
                Card dealerUpcard = new Card(upcardRankName, "D");

                AddColorBox(Colors.White, upcardRankName, x, 0);
                y = 1;

                for (int hardTotal = 20; hardTotal > 4; hardTotal--)
                {
                    // add a white box with the total
                    AddColorBox(Colors.White, hardTotal.ToString(), 0, y);

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
                            playerHand.AddCard(new Card("7D"));
                        }
                    }

                    playerHand.AddCard(new Card(firstCardRank, "D"));
                    playerHand.AddCard(new Card(secondCardRank, "S"));

                    // get strategy and display
                    var action = strategy.GetActionForHand(playerHand, dealerUpcard);
                    switch (action)
                    {
                        case ActionToTake.Hit:
                            AddColorBox(Colors.Green, "H", x, y);
                            break;

                        case ActionToTake.Stand:
                            AddColorBox(Colors.Red, "S", x, y);
                            break;

                        case ActionToTake.Double:
                            AddColorBox(Colors.Yellow, "D", x, y);
                            break;
                    }
                    y++;
                }
                x++;
            }

            // and another for hands with an ace
            // display a grid for hands without an ace.  One column for each possible dealer upcard
            const int leftColumnForAces = 12;
            AddColorBox(Colors.White, "", leftColumnForAces, 0);
            x = leftColumnForAces + 1;
            for (int upcardRank = 2; upcardRank < 12; upcardRank++)
            {
                string upcardRankName = (upcardRank == 11) ? "A" : upcardRank.ToString();
                Card dealerUpcard = new Card(upcardRankName, "D");

                AddColorBox(Colors.White, upcardRankName, x, 0);
                y = 1;

                // we don't start with Ace, because that would be AA, which is handled in the pair zone
                // we also don't start with 10, since that's blackjack.  So 9 is our starting point
                for (int otherCard = 9; otherCard > 1; otherCard--)
                {
                    string otherCardRank = (otherCard == 11) ? "A" : otherCard.ToString();

                    // add a white box with the player hand: "A-x"
                    AddColorBox(Colors.White, "A-" + otherCardRank, leftColumnForAces, y);

                    // build player hand
                    Hand playerHand = new Hand();
                    // first card is an ace, second card is looped over
                    playerHand.AddCard(new Card("AH")); // ace of hearts
                    playerHand.AddCard(new Card(otherCardRank, "S"));

                    // get strategy and display
                    var action = strategy.GetActionForHand(playerHand, dealerUpcard);
                    switch (action)
                    {
                        case ActionToTake.Hit:
                            AddColorBox(Colors.Green, "H", x, y);
                            break;

                        case ActionToTake.Stand:
                            AddColorBox(Colors.Red, "S", x, y);
                            break;

                        case ActionToTake.Double:
                            AddColorBox(Colors.Yellow, "D", x, y);
                            break;
                    }
                    y++;
                }
                x++;
            }

            // finally, a grid for pairs
            int startY = y + 1;
            AddColorBox(Colors.White, "", leftColumnForAces, 0);
            x = leftColumnForAces + 1;
            for (int upcardRank = 2; upcardRank < 12; upcardRank++)
            {
                string upcardRankName = (upcardRank == 11) ? "A" : upcardRank.ToString();
                Card dealerUpcard = new Card(upcardRankName, "D");

                AddColorBox(Colors.White, upcardRankName, x, 0);
                y = startY;

                for (int pairedCard = 11; pairedCard > 1; pairedCard--)
                {
                    string pairedCardRank = (pairedCard == 11) ? "A" : pairedCard.ToString();

                    // add a white box with the player hand: "x-x"
                    AddColorBox(Colors.White, pairedCardRank + "-" + pairedCardRank, leftColumnForAces, y);

                    // build player hand
                    Hand playerHand = new Hand();
                    playerHand.AddCard(new Card(pairedCardRank, "H")); // X of hearts
                    playerHand.AddCard(new Card(pairedCardRank, "S")); // X of spades

                    // get strategy and display
                    var action = strategy.GetActionForHand(playerHand, dealerUpcard);
                    switch (action)
                    {
                        case ActionToTake.Hit:
                            AddColorBox(Colors.Green, "H", x, y);
                            break;

                        case ActionToTake.Stand:
                            AddColorBox(Colors.Red, "S", x, y);
                            break;

                        case ActionToTake.Double:
                            AddColorBox(Colors.Yellow, "D", x, y);
                            break;

                        case ActionToTake.Split:
                            AddColorBox(Colors.LightBlue, "P", x, y);
                            break;
                    }
                    y++;
                }
                x++;
            }
        }

        private void AddColorBox(Color color, string label, int x, int y)
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