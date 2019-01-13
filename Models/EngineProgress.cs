using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackjackStrategy.Models
{
    class EngineProgress
    {
        public int GenerationNumber { get; set; }
        public float BestFitnessThisGen { get; set; }
        public float AvgFitnessThisGen { get; set; }
        public float BestFitnessSoFar { get; set; }
        public TimeSpan TimeForGeneration { get; set; }
    }
}
