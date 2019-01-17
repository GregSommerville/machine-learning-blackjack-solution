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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            gaResultTB.Text = "Creating solution, please wait...";

            //var comparisonStrategy = new HandCodedStrategy();
            //comparisonStrategy.LoadStandardStrategy();


            // Finding the solution takes a while, so kick off a thread for it
            Task.Factory.StartNew(() => AsyncCall());
        }

        private void AsyncCall()
        {
            progressMsg = new List<string>();

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
                gaResultTB.Text = "Solution found in " + totalGenerations + " gens\n\nTest Scores:\n" + scoreResults;
            }),
            DispatcherPriority.Background);
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
        private bool PerGenerationCallback(EngineProgress progress)
        {
            string summary =
                "Gen " + progress.GenerationNumber +
                " best: " + progress.BestFitnessThisGen.ToString("0") +
                " avg: " + progress.AvgFitnessThisGen.ToString("0");
            DisplayCurrentStatus(summary);

            Debug.WriteLine("Generation " + progress.GenerationNumber + " took " + progress.TimeForGeneration.TotalSeconds + " s");

            // all settings in one column
            string settings =
                "P: " + EngineParams.PopulationSize + " " +
                "G: " + EngineParams.MinGenerations + " - " + EngineParams.MaxGenerations + " " +
                "Stgn: " + EngineParams.MaxStagnantGenerations + " " +
                "Sel: " + EngineParams.SelectionStyle + " " +
                "Trny: " + EngineParams.TourneySize + " " +
                "Cross: " + EngineParams.CrossoverRate + " " +
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

            // return true to keep going, false to halt the system
            bool keepRunning = true;
            return keepRunning;
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


    }
}