using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlackjackStrategy.Models
{
    class StrategyPool
    {
        private object threadLock = new object();
        private Queue<Strategy> availablePool;
        private List<Strategy> inUseList;

        public StrategyPool(int populationSize)
        {
            // a queue to get the next available, a list to track which are used
            availablePool = new Queue<Strategy>(populationSize);
            inUseList = new List<Strategy>(populationSize);

            // and populate 
            for (int i = 0; i < populationSize; i++)
                availablePool.Enqueue(new Strategy());
        }

        public Strategy GetEmpty()
        {
            lock (threadLock)
            {
                Debug.Assert(availablePool.Any(), "Available queue empty");

                var result = availablePool.Dequeue();
                inUseList.Add(result);

                return result;
            }
        }

        public Strategy GetRandomized()
        {
            lock (threadLock)
            {
                Debug.Assert(availablePool.Any(), "Available queue empty");

                var result = availablePool.Dequeue();
                inUseList.Add(result);

                result.Randomize();
                return result;
            }
        }

        public Strategy CopyOf(Strategy strategy)
        {
            lock (threadLock)
            {
                Debug.Assert(availablePool.Any(), "Available queue empty");

                var result = availablePool.Dequeue();
                inUseList.Add(result);

                result.DeepCopy(strategy);
                return result;
            }
        }

        public void Release(Strategy strategy)
        {
            var found = inUseList.Find(s => s == strategy);
            Debug.Assert(found != null, "Error releasing strategy");

            inUseList.Remove(found);
            availablePool.Enqueue(found);
        }
    }
}
