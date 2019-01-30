using System.ComponentModel;

namespace BlackjackStrategy.Models
{
    public enum SelectionStyle { Tourney, Roulette, Ranked };

    [DisplayName("Genetic Algorithm Settings")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class EngineParameters
    {
        public int PopulationSize { get; set; } = 500;
        public int TourneySize { get; set; } = 3;
        public int MinGenerations { get; set; } = 50;
        public int MaxGenerations { get; set; } = 750;
        public int MaxStagnantGenerations { get; set; } = 25;
        public SelectionStyle SelectionStyle { get; set; } = Models.SelectionStyle.Tourney;  
        public double ElitismRate { get; set; } = 0.20;
        public double MutationRate { get; set; } = 0.10;
        public double MutationImpact { get; set; } = 0.05;

        // so it looks right in the property grid
        public override string ToString()
        {
            return "";
        }
    }
}
