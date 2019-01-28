# blackjack-solved-with-ai: A Genetic Algorithm Finds a Solution to Blackjack
 
## Introduction
This repo contains C# code that implements a genetic algorithm that finds an optimal strategy for the casino game known as Blackjack or 21.  The program is a Windows WPF application that allows you to play with different settings like population size, selection style and mutation rate.  Each generation's best solution is displayed, so you can watch the program literally evolve a solution.
 
## Genetic Algorithms
A genetic algorithm is a type of artificial intelligence programming that evolves a solution to a problem using concepts from biology like evolution.  The basic idea is to create a population of candidate solutions, where each candidate gets a fitness score based on a testing function.  Candidates with high fitness scores are combined using a method based on the concept of genetic crossover, and you can also include processing similar to genetic mutations to offspring of selected candidates.  Once a new generation is created, the previous generation is discarded and the process starts over again with the new generation.  Ultimately a final solution is found by choosing the best candidate over all of the generations.
 
Genetic algorithms (GAs) are extremely helpful when trying to find an optimal solution from a very large set of possible solutions.  For example, a Blackjack strategy is typically represented by three tables - one for hard hands, one for soft hands, and one for pairs.  Each cell of those three tables contains the value "Stand", "Hit", "Double-down", or (for pairs only) "Split".  
 
The total number of unique possible strategies is somewhere around 5 x 10<sup>175th</sup>.  Obviously that number is so large that any kind of brute-force approach would not work.  Luckily, GAs excel in this type of situation, and the program finds an optimal or near-optimal solution in just a few minutes, typically.
 
## Blackjack Rules and Strategies
The [rules of Blackjack](https://en.wikipedia.org/wiki/Blackjack) are fairly simple.  The dealer and the player both are dealt two cards.  The player sees both of their cards (they are usually dealt face up), and one of the dealer's cards is dealt face up.  Each card has a value - for cards between 2 and 10, the value is the same as the card's rank (so an Eight of Spades counts as 8, for example).  All face cards count as 10, and an Ace can either be 1 or 11 (it counts as 11 only when that does not result in a hand that exceeds 21).
 
After the cards are dealt, if the player has Blackjack (a total of 21) and the dealer does not, the player is immediately paid 1.5 times their original bet, and a new hand is dealt.  If the player has 21 and the dealer does also, then it's a tie and the player gets their original bet back, and a new hand is dealt.
 
If the player wasn't dealt a Blackjack, then play continues with the player deciding whether to Stand (not get any more cards), Hit (receive an additional card), Double-down (place an additional bet, and receive one and only one more card), or, in the case of holding a pair, splitting the hand, which means placing an additional bet and receiving two new cards, so the end result is that the player is now playing two (or, in the case of multiple splits, more than two) hands simultaneously.
 
If the player hits or double-downs and has a resulting hand that exceeds 21, then they lose and play continues with the next hand.  If not, then the dealer draws until their hand totals at least 17.  If the dealer exceeds 21 at this point, the player receives a payment equal to twice their original bet.  If the dealer doesn't exceed 21, then the hands are compared and the player with the highest total that doesn't exceed 21 wins.
 
Because of these rules, certain effective strategies emerge.  One common strategy is that if you hold a hard hand with a value of 20, 19 or 18, you should Stand, since you avoid busting by going over 21, and you have a nice hand total that might win in a showdown with the dealer.  
 
Another common strategy is to split a pair of Aces, since Aces are so powerful (due to the fact that count as 11 or 1, you can often Hit a hand with a soft Ace with no risk of busting).  Likewise, splitting a pair of 8s is a good idea because with a hard total of 16, it's likely you will bust if you take a Hit (since so many cards count as 10).
 
These strategies are often represented by three tables.  As was stated earlier, the columns of these tables refer to the dealer upcard, and the rows are for various player holdings like a pair of 6s, or a hard 12, etc.  Here's an example strategy table from Wikipedia:
![Wikipedia.org Blackjack Strategy image](/images/wikistrategy.jpg)
 
The GA program in this repo will find a strategy that is optimal or near-optimal, without any pre-programmed knowledge of Blackjack strategy.
 
## Representation
The first problem that most GA programs face is one of representation - in other words, how to represent a particular candidate solution.  In the case of Blackjack, the three tables can easily be represented by three 2-D arrays.  
 
Older implementations of a GA often tried to stay too close to the "genetic" part of the analogy, and often made representations a string of binary ones and zeros.  Frankly, there's no reason for this - representation should be whatever is appropriate to the problem, as long as it allows mutation and crossover operations.
 
## Fitness Functions
GAs are interesting because they are non-deterministic, meaning there is no way to determine if you've come up with the very best answer to a problem.  Instead, they rely on _relative_ comparisons between candidates, using something called a Fitness Function.  Each fitness function takes a candidate solution and returns a numeric value (typically, the higher the value, the better).  In the case of finding a Blackjack solution, it's quite simple - we just run thousands of hands of Blackjack using the candidate strategy and determine how much money they would have gained or lost by following that strategy.
 
One of the important questions in creating a fitness function is determining how many hands to play.  Certain hands (like being dealt a pair) are far less likely that others (like being dealt a hard hand).  Because of that, we need to ensure that we are testing the strategy with enough different starting hands to provide for a great deal of coverage.  In our case, we will start with testing each strategy with 25,000 different hands.  Since pairs are dealt only about 6%, that means 6% x 25000 = 1500 hands will be pairs.  Since there are 100 combinations of pairs and dealer upcards, that means each cell in our pairs table should be evaluated about 15 times each.
 
## Selection Styles
So once we know how we want to represent a candidate solution, and we know how to calculate a fitness function for each candidate, the next step is to select two candidates for crossover when building a new generation.  There are three common styles for selection, and this program supports all of them.
 
First, you can choose Roulette Wheel Selection.  It's named for a Roulette wheel because you can imagine each candidate's fitness score being a wedge in a pie chart, with a size proportionate to its relative fitness compared to the other candidates.  (Of course, this assumes that all fitness scores are positive, which we will talk about shortly).  The main benefit of Roulette Wheel selection is that selection is fitness-proportionate.  Imagine if you had only three candidates, with fitness scores of 1, 3, and 8.  The relative selection probabilities for those candidates will be 1/12, 3/12, and 8/12.  
 
The downside of Roulette Wheel selection is that it tends to be somewhat slow in terms of processing.  Although the fitness values don't need to be sorted to use Roulette Wheel, the selection process is done by iterating through the candidates until a particular condition is matched - in other words, O(N) performance.  
 
Another potential problem with Roulette Wheel selection is that there may be situations where fitness scores vary widely, to such an extent that only certain candidates have any reasonable chance of being selected.  This happens frequently in early generations, since the majority of candidates are mostly random.  Although this might sound like a positive (since you ultimately want to select candidates with high fitness scores), it also results in a loss of genetic diversity.  In other words, even though a particular candidate may have a low fitness score in an early generation, it may contain elements that are needed to find the ultimate solution in later generations.
 
Ranked Selection is the solution to this problem.  Instead of using raw fitness scores during the selection process, the candidates are sorted by fitness, with the worst candidate receiving a score of 1, the second worse receiving 2, and so forth, all the way to the best candidate, which has a score equal to the population size.  
 
Ranked Selection is quite slow, since it combines the O(N) performance of Roulette Wheel, with the additional requirement that the candidates be sorted before selection.  However, there may be circumstances where it performs better than other selection approaches.
 
Finally, the fastest selection method of all is called Tournament Selection.  This method simply selects N random candidates from the current generation, and then uses the one with the best fitness score.  If you have a small tournament size value (like 2), that means two random candidates are selected, and the best of those two is used.  If you have a large tournament size (like 20), then 20 different candidates will be selected, with the best of those being the ultimate selection.
 
Tournament selection works well in most cases, but it does require some experimentation to find the best tourney size.  This is a value that you can experiment with in the demonstration program.
 
## Elitism
Elitism is a technique that helps ensure that the best candidates are always maintained.  Since all selection methods are random to some degree, it is possible to completely lose the best candidates from one generation to another.  By using Elitism, we automatically advance a certain percentage of the best candidates to the next generation.  Elitism does have a negative impact on performance since all of the candidates must be sorted by fitness score.
 
Typically Elitism is done before filling the rest of a new generation with new candidates created by crossover.
 
## Crossover
Once two candidate solutions have been selected, the next step in building a new generation is to combine those two into a single new candidate, hopefully using the best of both parent strategies.  There are a number of ways to do crossover, but the method used in this program is quite straightforward - the two fitness scores are compared, and crossover happens in a relatively proportionate way.  So if one candidate had a fitness of 10, and the other had a fitness of 5, then the one with fitness 10 would contribute twice as much to the child as the parent with a fitness of 5.
 
Since the fitness scores in this program are based on how much the strategy would win over thousands of hands, almost all fitness scores will be negative.  Obviously, this makes it difficult to calculate relative fitnesses, and also causes problems with selection methods like Roulette Wheel or Ranked.  To solve this, we find the lowest fitness score of the generation and add that value to each candidate.  This results in an adjusted fitness score of 0 for the very worse candidate, so it never gets selected.
 
Finally, because we still want some genetic diversity regardless of the fitness scores of the two parents, we cap the relative proportions of the two fitness scores to 80-20, meaning that no matter how good one score is, and how bad the other is, at worst we still will draw a maximum of 80% of the strategy from the better parent.
 
## Mutation
As has been mentioned a few times, maintaining genetic diversity in our population of candidate solutions is a good thing.  It helps the GA ultimately find the very best solution, by occasionally altering a candidate in a positive direction.  For this program, the mutation is quite simple - one cell is randomly set in the pairs table, one cell is randomly set in the soft hands table, and (due to its size) two cells are randomly set in the hard hand table. 
 
The mutation rate is a number from 0 to 1 that controls whether or not we should mutate a candidate.  You should experiment with different values (including 0) to see what works best.
 
## Population Size
Along with other configuration values used in the GA, population size has a significant impact on performance.  The smaller the population size, the faster the GA will execute.  On the other hand, if the size is too low the population may not have enough genetic diversity to find the ultimate solution.
 
## Performance Notes
Multi-threading is a natural fit for genetic algorithms because we often want to perform the same action on each candidate.  The best example of this is when we calculate fitness scores.  This is often an operation that takes quite a bit of time.  In our case, we're dealing out 25,000 hands, and each hand has to be played until the end.  If we're single-threading that code, it's going to take a long time.  Multi-threading is really the way to go.
 
Luckily, there's a ridiculously simple way to efficiently use all of your processors for an operation like this.  This code loops over all of the candidates in the currentGeneration list, calls the fitness function and sets the fitness property for each:
 
                ```csharp
                Parallel.ForEach(currentGeneration, (candidate) =>
                {
                    // calc the fitness by calling the user-supplied function via the delegate   
                    float fitness = FitnessFunction(candidate);
                    candidate.Fitness = fitness;
                });
                ```
 
Regardless of the number of items in the list or the number of processors on your machine, the code will efficiently run the code in a multi-threaded manner, and continue only when all of the threads are complete.
 
During testing this code results in about 75% utilitization of 8 Xeon processors, which is much, much better than 95% of one processor.
 
One of the side effects of making this code multi-threaded is that all of the code relating to evaluating a candidate must be thread-safe, including any Singleton objects.  When making code thread-safe, pay attention that you don't accidentally introduce code that will slow your program down unintentionally, because sometimes it can be quite subtle.
 
Early on in the development of this project, the code in the fitness function that simulated thousands of hands of Blackjack created a random (shuffled) deck of cards for each hand, just to ensure randomness.  And, since most casinos shuffle 4 decks together, that's what the code does too, which results in 208 cards.  
 
In a multi-threaded environment, a random number generator must be a singleton, meaning there's one that's shared by all of the threads.  That's due to the way that random numbers generators work.
 
A random number generator uses a seed value, which is time-based, like the number of milliseconds the computer has been turned on.  Starting with that seed, subsequent calls will return a series of numbers that look random, but really aren't.  If you start with the same seed, you get the same sequence.  
 
And that's a problem because if you create multiple random number generator objects in a loop, for example, several of them will have the same time-based initial seed value, and will result in the same sequence of "random" numbers.  That's a bug, because it can reduce the true randomness of the program a great deal, and that's vital to a genetic algorithm.
 
However, serializing access to a randomizer singletons is necessary but also slows down our multi-threading, which means we should try to minimize (or spread out) calls to that object.
 
This was definitely a problem in one of the earlier versions of the code.  By having each candidate create a random deck for each hand, the Randomizer object was getting swamped.  Changing the code to create one deck per candidate, and automatically shuffle when 20 or less cards were left proved to be a huge improvement.  One particular run dropped from 1 minute 10 seconds per generation to 10 seconds per generation.
 
## Results
 
## Contributing
Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.
 
## Author
* **Greg Sommerville** - *Initial work* 
 
## License
This project is licensed under the Apache 2.0 License - see the [LICENSE.md](LICENSE.md) file for details