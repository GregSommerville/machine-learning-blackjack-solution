using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackjackStrategy.Models
{
    public enum SelectionStyle { Tourney, RouletteWheel, Ranked };

    public sealed class EngineParameters
    {
        public int? PopulationSize { get; set; } = 200;
        public int? TourneySize { get; set; } = 6;
        public int? MinGenerations { get; set; } = 25;
        public int? MaxGenerations { get; set; } = 125;
        public int? MaxStagnantGenerations { get; set; } = 16;
        public SelectionStyle? SelectionStyle { get; set; } = Models.SelectionStyle.Tourney;  // Tourney is usually fastest
        public double? ElitismRate { get; set; } = 0.10;
        public double? CrossoverRate { get; set; } = 1.00;
        public double? MutationRate { get; set; } = 0.03;
    }
}
