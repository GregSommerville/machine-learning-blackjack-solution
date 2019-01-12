# blackjack-solved-with-ai: A Genetic Algorithm Finds a Solution to Blackjack


Evolutionary.Net is an artificial intelligence code framework that allows you to easily write *genetic programs* using straightforward C# code.

Genetic programs are a form of *evolutionary computing*, where the computer evolves a solution to a problem using principles from natural selection, like survival of the fittest, genetic crossover, and even genetic mutation.

## Genetic Programming Basic Concepts

The ideas behind genetic programming originally came from a book called “Genetic Programming” by John Koza.  It described how populations of automatically generated programs could compete against each other, with the fittest programs selected for reproduction via genetic crossover.  Ultimately, after a number of generations, the best solution found is executable code that can be used like any other function.
 
To learn more about genetic programming, you have a few options.  

First, please see the Pluralsight.com course called [Understanding Genetic Algorithms and Genetic Programming](https://app.pluralsight.com/library/courses/genetic-algorithms-genetic-programming/table-of-contents).  The course was written by the author of Evolutionary.Net, and it explains the basis of the code and how it works.

Alternatively, for a shorter read, please see this very helpful free resource:  [Field Guide to Genetic Programming](http://www.gp-field-guide.org.uk/)

Here are some of the basic ideas behind genetic programming:
- With genetic programming, the idea is to let the computer discover the best solution to a problem, rather than having the programmer solve it
- The programmer guides the process by providing a function that indicates a candidate's fitness, and a set of components that can be used to build a candidate
- The Evolutionary.Net engine creates an initial population of candidate solutions that have been randomly initialized
- Each candidate solution has an expression tree, which is a tree structure that stores executable code
- The nodes of this expression tree can be several different types: *function nodes*, *constant nodes*, *variable nodes*, and *terminal function nodes*
- *Function nodes* have child nodes, which are used as the arguments to a function.  The values to be used are retrieved using recursion.
- *Variable nodes* are terminal nodes (meaning, they have no child nodes).  When a tree is built, the variable name is saved within a node.  Before the tree is evaluated, all variables must be given values.
- *Constant nodes* simply store a numeric or boolean value
- *Terminal function nodes* are functions that take no arguments.  They typically examine the problem state data in order to determine a return value
- The principle of _closure_ means that all nodes within an expression tree must return the same data type (like float or bool)
 
### What Are Genetic Programs Good For?
Due to the nature of a genetic program evolving, they are often well-suited for problems where there are many possible solutions.  In other words, if the solution space is very large, possibly due to combinatorial factors, a genetic program may be the best way to find a solution.
 
Often genetic programs are used when a solution is not immediately obvious to a human computer programmer.  One classic use case is finding a mathematical function or equation that fits a set of data.  By supplying a genetic engine a set of variables, constants, and math functions (like add, subtract, multiply, etc.), the end result will be code that fits the data closely, which is useful for predictions and trend detection.

### How Do Genetic Programs Work?
To create a genetic program, you supply the engine with the components (called the *primitive set*) that might be needed to solve a problem.  Then the engine creates an initial randomly generated population of candidate solutions.  
 
A candidate solution contains a tree data structure, with each node in the tree being either a constant (usually numeric, but it could be Boolean), a variable name (which is how the generated program implements parameters), or a function node.  Constant and variable nodes are known as terminal nodes since they don&#39;t have children, but functions usually do have children - one per parameter.  For example, a node for *add* would have two child nodes, and once each of those children are evaluated, the *add* node adds the values together and returns the sum total.
 
Evaluating a tree is done recursively.  First, the root node of the tree is evaluated, and if it&#39;s a function node (which it should be), then each of its children nodes is evaluated, and then the function operation (whatever it happens to be) is executed.  Because of this approach, the tree can be quite deep and full, with many nodes contributing to a final solution.
 
Each candidate is evaluated and is assigned a numeric fitness score.  You can have lower fitness scores indicate better solutions, but usually the traditional approach is that higher fitness scores indicate better solutions.
 
Then the engine selects candidate solutions two at a time for reproduction, via genetic crossover.  
 
## Installing

Download the code and compile using Visual Studio.  There are no dependencies in the code - it's just straight C#.  Any version of the .NET framework should be fine, although this hasn't been tested with versions prior to 4.5.  Then just add a reference to the Evolutionary.Net assembly to your program.

## Getting Started
The first step is to instantiate an engine object.  When you do that, you must specify two data types.  First, the data type for all of the nodes in the expression trees that will be generated is specified.  The concept that all nodes must share a single data type is called *closure*.  The second data type specified is an object to store state data.
 
After instantiating the engine object, you begin by defining the primitive set, which is a collection of constants, variables, functions and terminal functions that the genetic engine will use to create a program.
 
Adding constants is done by simply passing in the numeric or boolean constants you wish to be available during construction of an expression tree.
 
Variables are sometimes used to pass information from the calling program into the expression tree.  Adding them is done by calling `.AddVariable()`, passing in the name of the variable.  Then, before the expression tree is evaluated, you must set the value of the variables.
 
Adding a function is done by calling `.AddFunction()`, passing in a lambda function and a name for the function.  In this version of the Evolutionary.NET engine, functions can have up to four parameters each.

There's a second variation of adding a function, and that's a *stateful* function.  By calling `.AddStatefulFunction()`, you specify a function that will receive parameters like a regular function node does, but has an additional parameter that passes in the candidate's state data.
 
Terminal functions are a special case of functions - they have zero parameters (meaning, no child nodes).  These functions typically are used to set or get some other information related to the state of the problem, so each terminal function is passed a copy of the candidate's state data object.
 
Once the primitive set is defined, you call the engine and it returns the best solution found.  You can then copy the text representation of that solution into your own code, keeping in mind that you must provide implementations of any functions your expression tree uses.  Since those functions are usually quite simple, these functions are often small and simple.

### State Data

Terminal functions are often used to query information about the state of the problem.  If you have problem state information that doesn't fit neatly into a variable due to closure, you can store additional information in the state data and then use a stateful function or terminal function to query or manipulate that data.  An example of this would be a card game - the suits and ranks of a player's cards won't fit within a single-data typed expression tree, but it can be stored in the state data, where it can be examined or manipulated by stateful functions or terminal functions.

## Technical Documentation
For detailed technical documentation, please see the [Wiki](https://github.com/GregSommerville/Evolutionary.Net/wiki)

## Using Evolutionary.NET
There are two main chunks of code you&apos;ll need to write in order to use Evolutionary.Net.  

First, you instantiate the engine and define the primitive set:
```csharp
// set up the parameters for the engine
var engineParams = new EngineParameters()
{
	CrossoverRate = 0.95,
	ElitismPercentageOfPopulation = 15,
	IsLowerFitnessBetter = false,
	MutationRate = 0.15,
	PopulationSize = 750,
	TourneySize = 6,
	NoChangeGenerationCountForTermination = 10, 
	RandomTreeMinDepth = 5, 
	RandomTreeMaxDepth = 10
};

// due to closure, all nodes are the same type - float, in this case
// we also have to specify the type of our problem state data, which is the second type used
var engine = new Engine<float,ProblemState>(engineParams);

// we add variables via the name, and then set them in our fitness function (below)
engine.AddVariable("X");

// reasonable constants, since they combine well with multiplication, addition, etc. 
engine.AddConstant(0);
engine.AddConstant(-1);

// functions 
engine.AddFunction((a, b) => a + b, "Add");
engine.AddFunction((a, b) => a - b, "Sub");
engine.AddFunction((a, b) => a * b, "Mult");
engine.AddFunction((a, b) => (b == 0) ? 1 : a / b, "Div");

// Our fitness function is EvaluateCandidate.  Here's how we specify that
engine.AddFitnessFunction((t) => EvaluateCandidate(t));

// each generation we get a callback to show progress
engine.AddProgressFunction((t) => PerGenerationCallback(t));

// retrieve the best solution found and display
var bestSolution = engine.FindBestSolution();
Console.WriteLine("Best result is:");
Console.WriteLine(bestSolution.ToString());
```

Second, the thing that drives the evolutionary process is the fitness function.  Here's an example:

```csharp
private static float EvaluateCandidate(CandidateSolution<float,ProblemState> candidate)
{
    // run through our training data and see how close it is to the answers from the genetic program
    float totalDifference = 0;
    foreach (var dataPoint in testDataPoints)
    {
	// specify the value of our variable before evaluating this candidate
	candidate.SetVariableValue("X", dataPoint.X);
	float result = candidate.Evaluate();

	// now figure the difference between the calculated value and the training data
	float diff = Math.Abs(result - dataPoint.Y);
	totalDifference += diff;
    }

    // since the genetic engine doesn't stop while fitness scores are still improving,
    // speed things up by truncating the precision to 4 digits after the decimal
    totalDifference = (float)(Math.Truncate(totalDifference * 10000F) / 10000F);
    return totalDifference;
}
```
And finally, it usually takes a while to find a good answer, so for each generation, we'll get a callback with status information.  We can also terminate the looping by returning a false from this function.

```csharp
private bool PerGenerationCallback(EngineProgress progress)
{
    string summary = "Generation " + progress.GenerationNumber +
	" best: " + progress.BestFitnessThisGen.ToString("0") +
	" avg: " + progress.AvgFitnessThisGen.ToString("0");

    Debug.WriteLine(summary);

    // return true to keep going, false to halt the system
    bool keepRunning = true;
    return keepRunning;
}
```

There's not much in the problem state for this problem:

```csharp
class ProblemState
{
    // save state data in an object of this type before evaluating the expression tree
    // terminal functions can then look at it, and they or stateful functions can modify it, if desired
}

```

## Examples

The Blackjack example available for download shows how to use state information in conjunction with terminal functions like PlayerHolds16() to develop a strategy for Blackjack.  A genetic program is developed for each distinct dealer upcard.

Here's a screenshot of the resulting strategy for a run of the Blackjack program:

![blackjack strategy program screenshot](images/blackjack_screenshot1.png)

There are three tables shown.  The table on the left is for non-paired hard hand totals - that is, hands that are not paired and do not contain an Ace that can be 1 or 11.  If an Ace can be only 1 or 11 (due to the other cards), then you may use this table.  

The top table on the right side shows how to play a "soft" hand, which is a hard that contains an Ace that can be either 1 or 11.

The bottom table on the right side shows how to play a paired hand.  

For all of the tables, the values along the top (the column headers) are the dealer's upcard.  The cells in the tables contain "H" when you should Hit, "D" when you should Double-Down (only valid when holding two cards), "S" when you should Stand, and "P" when you should split (only valid when holding a pair). 
 
 
## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

 
## Authors

* **Greg Sommerville** - *Initial work* 
 
## License

This project is licensed under the Apache 2.0 License - see the [LICENSE.md](LICENSE.md) file for details