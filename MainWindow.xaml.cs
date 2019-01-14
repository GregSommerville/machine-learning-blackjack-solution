using BlackjackStrategy.Models;
using System;
using System.Collections.Generic;
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
        private List<string> progressSoFar = new List<string>();
        private int totalGenerations;

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

            // instantiate the engine with params, then set the callbacks for per-generation and for candidate evaluation
            var engine = new Engine(EngineParams);
            engine.ProgressCallback = PerGenerationCallback;
            engine.FitnessFunction = EvaluateCandidate;

            // and then let 'er rip
            var strategy = engine.FindBestSolution();

            // then display the final results
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // draw the grids
                StrategyView.ShowPlayableHands(strategy, canvas);

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

                gaResultTB.Text = "Solution found.\nScores:\n" + scoreResults;
            }),
            DispatcherPriority.Background);
        }

        //-------------------------------------------------------------------------
        // each candidate gets evaluated here
        //-------------------------------------------------------------------------
        private float EvaluateCandidate(Strategy candidate)
        {
            // then test that strategy and return the total money lost/made
            var strategyTester = new StrategyTester(candidate);
            return strategyTester.GetStrategyScore(TestConditions.NumHandsToPlay);
        }

        //-------------------------------------------------------------------------
        // For each generation, we get information about what's going on
        //-------------------------------------------------------------------------
        private bool PerGenerationCallback(EngineProgress progress)
        {
            string summary = "Generation " + progress.GenerationNumber +
                " best: " + progress.BestFitnessThisGen.ToString("0") +
                " avg: " + progress.AvgFitnessThisGen.ToString("0");
            DisplayCurrentStatus(summary);

            // keep track of how many gens we've searched
            totalGenerations = progress.GenerationNumber;

            // return true to keep going, false to halt the system
            bool keepRunning = true;
            return keepRunning;
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


    }
}