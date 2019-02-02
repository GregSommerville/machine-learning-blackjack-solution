using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackjackStrategy.Models
{
    class StrategyFactory
    {
        private Queue<Strategy> availablePool, inUsePool;

        public StrategyFactory(int populationSize)
        {
            // set up the queues
            availablePool = new Queue<Strategy>(populationSize);
            inUsePool = new Queue<Strategy>(populationSize);

            // and populate 
            for (int i = 0; i < populationSize; i++)
                availablePool.Enqueue(new Strategy());
        }

        public Strategy GetEmpty()
        {
            var result = availablePool.Dequeue();
            inUsePool.Enqueue(result);

            return result;
        }

        public Strategy GetRandomized()
        {
            var result = availablePool.Dequeue();
            inUsePool.Enqueue(result);

            result.Randomize();
            return result;
        }

        public Strategy CopyOf(Strategy strategy)
        {
            var result = availablePool.Dequeue();
            inUsePool.Enqueue(result);

            result.DeepCopy(strategy);
            return result;
        }

        public void Reset()
        {
            while (inUsePool.Count > 0)
                availablePool.Enqueue(inUsePool.Dequeue());
        }
    }
}
