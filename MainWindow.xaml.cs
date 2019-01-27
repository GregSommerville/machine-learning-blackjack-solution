using BlackjackStrategy.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
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
        private List<string> progressMsg = new List<string>();
        private int totalGenerations;
        private float bestOverallScoreSoFar, bestAverageScoreSoFar;
        private Stopwatch stopwatch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            gaResultTB.Text = "Creating solution, please wait...";

            bestOverallScoreSoFar = float.MinValue;
            bestAverageScoreSoFar = float.MinValue;
            stopwatch.Restart();

            // Finding the solution takes a while, so kick off a thread for it
            Task.Factory.StartNew(() => AsyncFindAndDisplaySolution());
        }

        private void AsyncFindAndDisplaySolution()
        {
            progressMsg = new List<string>();

            // instantiate the engine with params, then set the callbacks for per-generation and for candidate evaluation
            var engine = new Engine(EngineParams);
            engine.ProgressCallback = PerGenerationCallback;
            engine.FitnessFunction = EvaluateCandidate;

            // and then let 'er rip
            var strategy = engine.FindBestSolution();
            DisplayStrategyGrids(strategy);
            DisplayStatistics(strategy);
        }

        //-------------------------------------------------------------------------
        // each candidate gets evaluated here
        //-------------------------------------------------------------------------
        private float EvaluateCandidate(Strategy candidate)
        {
            // test the strategy and return the total money lost/made
            var strategyTester = new StrategyTester(candidate);
            return strategyTester.GetStrategyScore(TestConditions.NumHandsToPlay);
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
                "  avg: " + progress.AvgFitnessThisGen.ToString("0").PadLeft(7) + avgSuffix;
                
            DisplayCurrentStatus(summary);

            Debug.WriteLine("Generation " + progress.GenerationNumber + 
                " took " + progress.TimeForGeneration.TotalSeconds.ToString("0") + "s");

            // all settings in one column
            string settings =
                "P: " + EngineParams.PopulationSize + " " +
                "G: " + EngineParams.MinGenerations + " - " + EngineParams.MaxGenerations + " " +
                "Stgn: " + EngineParams.MaxStagnantGenerations + " " +
                "Sel: " + EngineParams.SelectionStyle + " " +
                "Trny: " + EngineParams.TourneySize + " " +
                "Mut: " + EngineParams.MutationRate + " " +
                "Elit: " + EngineParams.ElitismRate + " ";

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

            // then display the final results
            DisplayStrategyGrids(bestThisGen);

            // return true to keep going, false to halt the system
            bool keepRunning = true;
            return keepRunning;
        }

        private void DisplayStrategyGrids(Strategy strategy)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                StrategyView.ShowPlayableHands(strategy, canvas);
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

        private void DisplayStatistics(Strategy strategy)
        {
            // then display the final results
            Dispatcher.BeginInvoke(new Action(() =>
            {
                stopwatch.Stop();

                // test it and display scores
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
                gaResultTB.Text = "Solution found in " + totalGenerations + " generations\nElapsed: " +
                    stopwatch.Elapsed.Hours + "h " +
                    stopwatch.Elapsed.Minutes + "m " +
                    stopwatch.Elapsed.Seconds + "s " +
                    "\n\nTest Scores:\n" + scoreResults;
            }),
            DispatcherPriority.Background);

        }
    }
}