namespace BlackjackStrategy.Models
{
    public enum SelectionStyle { Tourney, RouletteWheel, Ranked };

    public sealed class EngineParameters
    {
        public int? PopulationSize { get; set; } = 500;
        public int? TourneySize { get; set; } = 2;
        public int? MinGenerations { get; set; } = 25;
        public int? MaxGenerations { get; set; } = 250;
        public int? MaxStagnantGenerations { get; set; } = 15;
        public SelectionStyle? SelectionStyle { get; set; } = Models.SelectionStyle.Tourney;  // Tourney is usually fastest, Roulette is proportionate
        public double? ElitismRate { get; set; } = 0.20;
        public double? MutationRate { get; set; } = 0.10;
    }
}
