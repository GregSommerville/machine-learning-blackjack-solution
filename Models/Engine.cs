using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackjackStrategy.Models
{
    class Engine<T>
    {
        public Func<T, float> FitnessFunction { get; set; }
        public Func<EngineProgress, bool> ProgressCallback { get; set; }

        private EngineParameters currentEngineParams = new EngineParameters();  // with defaults

        public Engine(EngineParameters userParams)
        {
            // user parameters may override default parameters
            if (userParams.CrossoverRate.HasValue)              currentEngineParams.CrossoverRate =             userParams.CrossoverRate.Value;
            if (userParams.ElitismRate.HasValue)                currentEngineParams.ElitismRate =               userParams.ElitismRate.Value;
            if (userParams.MaxGenerations.HasValue)             currentEngineParams.MaxGenerations =            userParams.MaxGenerations.Value;
            if (userParams.MinGenerations.HasValue)             currentEngineParams.MinGenerations =            userParams.MinGenerations.Value;
            if (userParams.MutationRate.HasValue)               currentEngineParams.MutationRate =              userParams.MutationRate.Value;
            if (userParams.MaxStagnantGenerations.HasValue)     currentEngineParams.MaxStagnantGenerations =    userParams.MaxStagnantGenerations.Value;
            if (userParams.PopulationSize.HasValue)             currentEngineParams.PopulationSize =            userParams.PopulationSize.Value;
            if (userParams.TourneySize.HasValue)                currentEngineParams.TourneySize =               userParams.TourneySize.Value;
            if (userParams.SelectionStyle.HasValue)             currentEngineParams.SelectionStyle =            userParams.SelectionStyle.Value;
        }

        public T FindBestSolution()
        {
            throw new NotImplementedException();
        }

    }
}
