# blackjack-solved-with-ai: A Genetic Algorithm Finds a Solution to Blackjack

## Performance Notes

Multi-threading is a natural fit for genetic algorithms, because we often want to perform the same action on each candidate separately.  The best example of this is when we calculate fitness scores.  This is often an operation that takes quite a bit of time.  In our case, we're dealing out 25,000 hands, and each hand has to be played until the end.  If we're stuck single-threading that code, it's going to take a long time.  Multi-threading is really the way to go.

Luckily, there's a ridiculously simple way to efficiently use all of your processors for an operation like this.  This code:

                Parallel.ForEach(currentGeneration, (candidate) =>
                {
                    // calc the fitness by calling the user-supplied function via the delegate   
                    float fitness = FitnessFunction(candidate);
                    candidate.Fitness = fitness;
                });

loops over all of the candidates in the currentGeneration list, calls the fitness function, and sets the fitness property.  Regardless of the number of items in the list, the code will efficiently run the code in a multi-threaded manner, and continue only when all of the threads are complete.

Running this code, I get about 73% utilitization of my 8 Xeon processors, which is much better than 95% of one processor.

Obviously, all of the code relating to evaluating a candidate must be thread-safe, including any singleton objects.  And when making code thread-safe, pay attention that you don't accidentally introduce code that will slow your program down unintentionally, because sometimes it can be quite subtle.

Early on in the development of this project, the code in the fitness function that simulated thousands of hands of Blackjack created a random (shuffled) deck of cards for each hand, just to ensure randomness.  And, since most casinos shuffle 4 decks together, that's what the code does too, which results in 208 cards.  

In a multi-threaded environment, a random number generator must be a singleton, meaning there's one that's shared by all of the threads.  That's due to the way that random numbers generators work.

A random number generator uses a seed value, which is time-based, like the number of milliseconds the computer has been turned on.  Starting with that seed, subsequent calls will return a series of numbers that look random, but really aren't.  If you start with the same seed, you get the same sequence.  

And that's a problem because if you create multiple random number generator objects in a loop, for example, several of them will have the same time-based initial seed value, and will result in the same sequence of "random" numbers.  That's a bug, because it can reduce the true randomness of the program a great deal, and that's vital to a genetic algorithm.

However, serializing access to a randomizer singletons is necessary but also slows down our multi-threading, which means we should try to minimize (or spread out) calls to that object.

This was definitely a problem in one of the earlier versions of the code.  By having each candidate create a random deck for each hand, the Randomizer object was getting swamped.  Changing the code to create one deck per candidate, and automatically shuffle when 20 or less cards were left proved to be a huge improvement.  One particular run dropped from 1 minute 10 seconds per generation to 10 seconds per generation.



 
## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

 
## Authors

* **Greg Sommerville** - *Initial work* 
 
## License

This project is licensed under the Apache 2.0 License - see the [LICENSE.md](LICENSE.md) file for details