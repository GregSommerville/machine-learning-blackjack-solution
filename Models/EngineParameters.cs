using System.ComponentModel;

namespace BlackjackStrategy.Models
{
    public enum SelectionStyle { Tourney, Roulette, Ranked };

    [DisplayName("Genetic Algorithm Settings")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class EngineParameters
    {
        [Description("Number of candidates per generation")]
        public int PopulationSize { get; set; } = 750;

        [Description("How parents are selected for crossover")]
        public SelectionStyle SelectionStyle { get; set; } = Models.SelectionStyle.Tourney;

        [Description("If using Tourney Selection, how many to select")]
        public int TourneySize { get; set; } = 3;

        [Description("Min number of generations it must run")]
        public int MinGenerations { get; set; } = 50;

        [Description("Max number of generations it can run")]
        public int MaxGenerations { get; set; } = 500;

        [Description("Execution stops after this # gens with no improvement in best or average scores")]
        public int MaxStagnantGenerations { get; set; } = 25;

        [Description("From 0.0 to 1.0, percentage of the best scoring candidates moved to the next generation")]
        public double ElitismRate { get; set; } = 0;

        [Description("From 0.0 to 1.0, percentage of candidates that are mutated")]
        public double MutationRate { get; set; } = 0;

        [Description("From 0.0 to 1.0, percentage of table cells that get mutated")]
        public double MutationImpact { get; set; } = 0.10;

        // so it looks right in the property grid
        public override string ToString()
        {
            return "";
        }
    }
}
