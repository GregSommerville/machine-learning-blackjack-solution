using System;
using System.Threading;

namespace BlackjackStrategy.Models
{
    public class Randomizer
    {
        private Random randomizer;

        public Randomizer()
        {
            // this is a pretty good way to get a random-ish seed value, so each thread
            // is different, even if created at roughly the same moment
            randomizer = new Random(Guid.NewGuid().GetHashCode());
        }

        /// <summary>
        /// Inclusive, random value returned may include either upper or lower boundary.  Safe for multi-threading.
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public int IntBetween(int lower, int upper)
        {
            return randomizer.Next(lower, upper);
        }

        /// <summary>
        /// Returns an int up to, but not including the parameter.  Safe for multi-threading.
        /// </summary>
        /// <param name="upper"></param>
        /// <returns></returns>
        public int IntLessThan(int upper)
        {
            return randomizer.Next(upper);
        }

        /// <summary>
        /// Returns a double >= 0 and less than 1.0.  Safe for multi-threading.
        /// </summary>
        /// <returns></returns>
        public double GetDoubleFromZeroToOne()
        {
            return randomizer.NextDouble();
        }

        /// <summary>
        /// Returns a float >= 0 and less than 1.0.  Safe for multi-threading.
        /// </summary>
        /// <returns></returns>
        public float GetFloatFromZeroToOne()
        {
            return (float)randomizer.NextDouble();
        }
    }
}
