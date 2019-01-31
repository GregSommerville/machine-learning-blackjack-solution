using System;
using System.Threading;

namespace BlackjackStrategy.Models
{
    public class Randomizer
    {
        [ThreadStatic] private static Random randomizer;

        // Using a [ThreadStatic] attribute means we need to check for initialization
        private static void CreateIfNeeded()
        {
            if (randomizer == null)
                randomizer = new Random(Guid.NewGuid().GetHashCode());
        }

        /// <summary>
        /// Inclusive, random value returned may include either upper or lower boundary.  Safe for multi-threading.
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static int IntBetween(int lower, int upper)
        {
            CreateIfNeeded();
            return randomizer.Next(lower, upper);
        }

        /// <summary>
        /// Returns an int up to, but not including the parameter.  Safe for multi-threading.
        /// </summary>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static int IntLessThan(int upper)
        {
            CreateIfNeeded();
            return randomizer.Next(upper);
        }

        /// <summary>
        /// Returns a double >= 0 and less than 1.0.  Safe for multi-threading.
        /// </summary>
        /// <returns></returns>
        public static double GetDoubleFromZeroToOne()
        {
            CreateIfNeeded();
            return randomizer.NextDouble();
        }

        /// <summary>
        /// Returns a float >= 0 and less than 1.0.  Safe for multi-threading.
        /// </summary>
        /// <returns></returns>
        public static float GetFloatFromZeroToOne()
        {
            CreateIfNeeded();
            return (float)randomizer.NextDouble();
        }
    }
}
