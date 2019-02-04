using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BlackjackStrategy.Models
{
    class Engine
    {
        public Func<Strategy, float> FitnessFunction { get; set; }
        public Func<EngineProgress, Strategy, bool> ProgressCallback { get; set; }
        public Strategy BestSolution { get; set; }
        public int NumGenerationsNeeded { get; set; }

        private EngineParameters currentEngineParams = new EngineParameters();  // with defaults
        private List<Strategy> currentGeneration = new List<Strategy>();
        private StrategyPool pool;
        private float totalFitness = 0;      

        public Engine(EngineParameters userParams)
        {
            currentEngineParams = userParams;
        }

        public Strategy FindBestSolution()
        {
            // this code assumes that a "best" fitness is one with the highest fitness score
            float bestFitnessScoreAllTime = float.MinValue;
            float bestAverageFitnessScore = float.MinValue;
            int bestSolutionGenerationNumber = 0, bestAverageFitnessGenerationNumber = 0;

            // a list of references to objects in our pool
            List<Strategy> nextGeneration = new List<Strategy>();

            // use a pool of candidates, so we aren't constantly creating and destroying
            pool = new StrategyPool(currentEngineParams.PopulationSize * 2);    // *2 to cover this gen and next

            // elitism
            int numElitesToAdd = (int)(currentEngineParams.ElitismRate * currentEngineParams.PopulationSize);

            // depending on whether elitism is used, or the selection type, we may need to sort candidates by fitness (which is slower)
            bool needToSortByFitness =
                currentEngineParams.SelectionStyle == SelectionStyle.Roulette ||
                currentEngineParams.SelectionStyle == SelectionStyle.Ranked ||
                currentEngineParams.ElitismRate > 0;

            // initialize generation 0 with randomness
            for (int n = 0; n < currentEngineParams.PopulationSize; n++)
            {
                var strategy = pool.GetRandomized();
                currentGeneration.Add(strategy);
            }

            // loop over generations
            Stopwatch stopwatch = new Stopwatch();
            int currentGenerationNumber = 0;
            while (true)
            {
                stopwatch.Restart();

                // for each candidate, find and store the fitness score
                Parallel.ForEach(currentGeneration, (candidate) =>
                {
                    // calc the fitness by calling the user-supplied function via the delegate   
                    candidate.Fitness = FitnessFunction(candidate);
                });

                // now check if we have a new best
                float bestFitnessScoreThisGeneration = float.MinValue;
                Strategy bestSolutionThisGeneration = null;
                float totalFitness = 0;

                foreach (var candidate in currentGeneration)
                {
                    totalFitness += candidate.Fitness;

                    // find best of this generation, update best all-time if needed
                    bool isBestThisGeneration = candidate.Fitness > bestFitnessScoreThisGeneration;
                    if (isBestThisGeneration)
                    {
                        bestFitnessScoreThisGeneration = candidate.Fitness;
                        bestSolutionThisGeneration = candidate;

                        bool isBestEver = bestFitnessScoreThisGeneration > bestFitnessScoreAllTime;
                        if (isBestEver)
                        {
                            bestFitnessScoreAllTime = bestFitnessScoreThisGeneration;
                            BestSolution = candidate.Clone();
                            bestSolutionGenerationNumber = currentGenerationNumber;
                        }
                    }
                }

                // determine average fitness and store if it's all-time best
                float averageFitness = totalFitness / currentEngineParams.PopulationSize;
                if (averageFitness > bestAverageFitnessScore)
                {
                    bestAverageFitnessGenerationNumber = currentGenerationNumber;
                    bestAverageFitnessScore = averageFitness;
                }

                // report progress back to the user, and allow them to terminate the loop
                EngineProgress progress = new EngineProgress()
                {
                    GenerationNumber = currentGenerationNumber,
                    AvgFitnessThisGen = averageFitness,
                    BestFitnessThisGen = bestFitnessScoreThisGeneration,
                    BestFitnessSoFar = bestFitnessScoreAllTime,
                    TimeForGeneration = stopwatch.Elapsed
                };
                bool keepGoing = ProgressCallback(progress, bestSolutionThisGeneration);
                if (!keepGoing) break;  // user signalled to end looping

                // termination conditions
                if (currentGenerationNumber >= currentEngineParams.MinGenerations)
                {
                    // exit the loop if we're not making any progress in our average fitness score or our overall best score
                    if (((currentGenerationNumber - bestAverageFitnessGenerationNumber) >= currentEngineParams.MaxStagnantGenerations) &&
                        ((currentGenerationNumber - bestSolutionGenerationNumber) >= currentEngineParams.MaxStagnantGenerations))
                        break;

                    // maxed out?
                    if (currentGenerationNumber >= currentEngineParams.MaxGenerations)
                        break;
                }

                // depending on the SelectionStyle, we may need to adjust all candidate's fitness scores
                AdjustFitnessScores(needToSortByFitness);

                // Start building the next generation
                nextGeneration.Clear();

                // Elitism
                var theBest = currentGeneration.Take(numElitesToAdd);
                foreach (var peakPerformer in theBest)
                    nextGeneration.Add(pool.CopyOf(peakPerformer));

                //// if we're doing elitism and the all-time best is from a previous generation, add it 
                if ((numElitesToAdd > 0) && (currentGenerationNumber != bestSolutionGenerationNumber))
                    nextGeneration.Add(pool.CopyOf(BestSolution));

                // then do the selection, crossover and mutation to populate the rest of the next generation
                var children = SelectCrossOverAndMutate(currentEngineParams.PopulationSize - nextGeneration.Count);
                nextGeneration.AddRange(children);

                // move to the next generation
                foreach (var strategy in currentGeneration)
                    pool.Release(strategy);
                currentGeneration.Clear();
                currentGeneration.AddRange(nextGeneration);
                
                currentGenerationNumber++;
            }

            return BestSolution;
        }

        private void AdjustFitnessScores(bool needToSortByFitness)
        {
            // we may need to sort the current generation by fitness, depending on SelectionStyle
            if (needToSortByFitness)
                currentGeneration = currentGeneration.OrderByDescending(c => c.Fitness).ToList();

            // if doing ranked, adjust the fitness scores to be the ranking, with 0 = worst, (N-1) = best.
            // this style is good if the fitness scores for different candidates in the same generation would vary widely, especially
            // in early generations.  It smooths out those differences, which allows more genetic diversity
            if (currentEngineParams.SelectionStyle == SelectionStyle.Ranked)
            {
                float fitness = currentGeneration.Count - 1;
                foreach (var candidate in currentGeneration)
                    candidate.Fitness = fitness--;
            }

            // and calc total for two kinds of selections
            totalFitness = 0;
            if (currentEngineParams.SelectionStyle == SelectionStyle.Roulette || 
                currentEngineParams.SelectionStyle == SelectionStyle.Ranked)
            {
                float smallestFitness = currentGeneration.Min(c => c.Fitness);
                float addToEach = 0;
                if (smallestFitness < 0)
                {
                    // need to add the minimum so we have all positive fitnesses
                    addToEach = Math.Abs(smallestFitness);
                }

                foreach (var candidate in currentGeneration)
                {
                    candidate.Fitness += addToEach;                    
                    totalFitness += candidate.Fitness;
                }
            }
        }

        private Strategy[] SelectCrossOverAndMutate(int numNeeded)
        {
            // multi-threading to fill in the remaining children for the next generation
            ConcurrentBag<Strategy> results = new ConcurrentBag<Strategy>();
            Parallel.For(0, numNeeded, (i) =>
                {
                    var randomizer = new Randomizer();  // thread-specific version

                    // select parents
                    Strategy parent1 = null, parent2 = null;
                    switch (currentEngineParams.SelectionStyle)
                    {
                        case SelectionStyle.Tourney:
                            parent1 = TournamentSelectParent();
                            parent2 = TournamentSelectParent();
                            break;

                        case SelectionStyle.Roulette:
                        case SelectionStyle.Ranked:
                            parent1 = RouletteSelectParent();
                            parent2 = RouletteSelectParent();
                            break;
                    }

                    // cross them over to generate a new child
                    Strategy child = pool.GetEmpty();
                    parent1.CrossOverWith(parent2, child);

                    // Mutation
                    if (randomizer.GetFloatFromZeroToOne() < currentEngineParams.MutationRate)
                        child.Mutate(currentEngineParams.MutationImpact);

                    results.Add(child);
                });

            return results.ToArray();
        }

        // Selection Routines -----------------------------------------------

        private Strategy TournamentSelectParent()
        {
            var randomizer = new Randomizer();  // thread-specific version

            Strategy result = null;
            float bestFitness = float.MinValue;

            for (int i = 0; i < currentEngineParams.TourneySize; i++)
            {
                int index = randomizer.IntLessThan(currentEngineParams.PopulationSize);
                var randomCandidate = currentGeneration[index];
                var fitness = randomCandidate.Fitness;

                bool isFitnessBetter = fitness > bestFitness;
                if (isFitnessBetter)
                {
                    result = randomCandidate;
                    bestFitness = fitness;
                }
            }
            return result;
        }

        private Strategy RouletteSelectParent()
        {
            var randomizer = new Randomizer();  // thread-specific version

            // using Roulette Wheel Selection, we grab a possibility proportionate to it's fitness compared to
            // the total fitnesses of all possibilities
            double randomValue = randomizer.GetDoubleFromZeroToOne() * totalFitness;
            for (int i = 0; i < currentEngineParams.PopulationSize; i++)
            {
                randomValue -= currentGeneration[i].Fitness;
                if (randomValue <= 0)
                {
                    return currentGeneration[i];
                }
            }

            return currentGeneration[currentEngineParams.PopulationSize - 1];
        }
    }
}
