using BlackjackStrategy.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Linq;

namespace BlackjackStrategy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // This parameters object is bound to the UI, for editing
        public ProgramSettings ProgramConfiguration { get; set; } = new ProgramSettings();

        // each callback adds a progress string here 
        private List<string> progressMsg = new List<string>();
        private int totalGenerations;
        private float bestOverallScoreSoFar, bestAverageScoreSoFar;
        private Stopwatch stopwatch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
            propGrid.ExpandAllProperties();
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            gaResultTB.Text = "Creating solution, please wait...";

            bestOverallScoreSoFar = float.MinValue;
            bestAverageScoreSoFar = float.MinValue;
            stopwatch.Restart();

            SetButtonsEnabled(false);

            // Finding the solution takes a while, so kick off a thread for it
            Task.Factory.StartNew(() => AsyncFindAndDisplaySolution());
        }

        private void btnShowKnown_Click(object sender, RoutedEventArgs e)
        {
            var strategy = new HandCodedStrategy();
            strategy.LoadStandardStrategy();
            DisplayStrategyGrids(strategy, "Classic Baseline Blackjack Strategy");
            DisplayStatistics(strategy);
        }

        private void AsyncFindAndDisplaySolution()
        {
            progressMsg = new List<string>();

            // instantiate the engine with params, then set the callbacks for per-generation and for candidate evaluation
            var engine = new Engine(ProgramConfiguration.GAsettings);
            engine.ProgressCallback = PerGenerationCallback;
            engine.FitnessFunction = EvaluateCandidate;

            // and then let 'er rip
            var strategy = engine.FindBestSolution();
            DisplayStrategyGrids(strategy, "Best from " + totalGenerations + " generations");
            DisplayStatistics(strategy);

            SetButtonsEnabled(true);
        }

        //-------------------------------------------------------------------------
        // each candidate gets evaluated here
        //-------------------------------------------------------------------------
        private float EvaluateCandidate(Strategy candidate)
        {
            // test the strategy and return the total money lost/made
            var strategyTester = new StrategyTester(candidate, ProgramConfiguration.TestSettings);
            strategyTester.StackTheDeck = ProgramConfiguration.TestSettings.StackTheDeck;

            return strategyTester.GetStrategyScore(ProgramConfiguration.TestSettings.NumHandsToPlay);
        }

        //-------------------------------------------------------------------------
        // For each generation, we get information about what's going on
        //-------------------------------------------------------------------------
        private bool PerGenerationCallback(EngineProgress progress, Strategy bestThisGen)
        {
            string bestSuffix = " ", avgSuffix = " ";
            if (progress.BestFitnessSoFar > bestOverallScoreSoFar)
            {
                bestOverallScoreSoFar = progress.BestFitnessSoFar;
                bestSuffix = "*";
            }
            if (progress.AvgFitnessThisGen > bestAverageScoreSoFar)
            {
                bestAverageScoreSoFar = progress.AvgFitnessThisGen;
                avgSuffix = "*";
            }

            string summary =
                "Gen " + progress.GenerationNumber.ToString().PadLeft(4) +
                "  best: " + progress.BestFitnessThisGen.ToString("0").PadLeft(7) + bestSuffix +
                "  avg: " + progress.AvgFitnessThisGen.ToString("0").PadLeft(7) + avgSuffix +
                "    " + progress.TimeForGeneration.TotalSeconds.ToString("0") + "s"; 
                
            DisplayCurrentStatus(summary);

            Debug.WriteLine("Generation " + progress.GenerationNumber + 
                " took " + progress.TimeForGeneration.TotalSeconds.ToString("0") + "s");

            // all settings in one column
            string settings =
                "P: " + ProgramConfiguration.GAsettings.PopulationSize + " " +
                "G: " + ProgramConfiguration.GAsettings.MinGenerations + " - " + ProgramConfiguration.GAsettings.MaxGenerations + " " +
                "Stgn: " + ProgramConfiguration.GAsettings.MaxStagnantGenerations + " " +
                "Sel: " + ProgramConfiguration.GAsettings.SelectionStyle + " " +
                "Trny: " + ProgramConfiguration.GAsettings.TourneySize + " " +
                "Mut: " + ProgramConfiguration.GAsettings.MutationRate + " " +
                "MI: " + ProgramConfiguration.GAsettings.MutationImpact + " " + 
                "Elit: " + ProgramConfiguration.GAsettings.ElitismRate + " ";

            // save stats: date, gen#, best-this-gen, avg-this-gen, settings
            var writer = File.AppendText("per-gen-stats.csv");
            writer.WriteLine(
                "\"" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\"," +
                progress.GenerationNumber + "," +
                progress.BestFitnessThisGen.ToString("0") + "," +
                progress.AvgFitnessThisGen.ToString("0") + "," + 
                settings);
            writer.Close();

            // keep track of how many gens we've searched
            totalGenerations = progress.GenerationNumber;

            // then display this generation's best 
            DisplayStrategyGrids(bestThisGen, "Best from generation " + totalGenerations);

            // return true to keep going, false to halt the system
            bool keepRunning = true;
            return keepRunning;
        }

        private void DisplayStrategyGrids(StrategyBase strategy, string caption)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                string imgFilename = "";
                if (ProgramConfiguration.TestSettings.SaveImagePerGeneration)
                    imgFilename = "gen" + totalGenerations;

                StrategyView.ShowPlayableHands(strategy, canvas, imgFilename, caption);
            }),
            DispatcherPriority.Background);
        }

        private void DisplayCurrentStatus(string status)
        {
            progressMsg.Insert(0, status);
            string allStatuses = String.Join("\n", progressMsg);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                gaResultTB.Text = allStatuses;
            }),
            DispatcherPriority.Background);
        }

        private void SetButtonsEnabled(bool enable)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                btnSolve.IsEnabled = enable;
                btnShowKnown.IsEnabled = enable;
            }),
            DispatcherPriority.Background);
        }

        private void DisplayStatistics(StrategyBase strategy)
        {
            // then display the final results
            Dispatcher.BeginInvoke(new Action(() =>
            {
                stopwatch.Stop();

                // test it and display scores
                var tester = new StrategyTester(strategy, ProgramConfiguration.TestSettings);

                double average, stdDev, coeffVariation;
                tester.GetStatistics(out average, out stdDev, out coeffVariation);

                string scoreResults =
                    "\nAverage score: " + average.ToString("0") +
                    "\nStandard Deviation: " + stdDev.ToString("0") +
                    "\nCoeff. of Variation: " + coeffVariation.ToString("0.0000");

                gaResultTB.Text = "Solution found in " + totalGenerations + " generations\nElapsed: " +
                    stopwatch.Elapsed.Hours + "h " +
                    stopwatch.Elapsed.Minutes + "m " +
                    stopwatch.Elapsed.Seconds + "s " +
                    "\n\nTest Results:" + scoreResults;
            }),
            DispatcherPriority.Background);
        }
    }
}