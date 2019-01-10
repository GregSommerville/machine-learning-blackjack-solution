using System;

namespace BlackjackStrategy.Models
{
    public class Randomizer
    {
        private static Random randomizer = new Random();

        /// <summary>
        /// Inclusive, random value returned may include either upper or lower boundary.  Safe for multi-threading.
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static int IntBetween(int lower, int upper)
        {
            lock (randomizer)
            {
                return randomizer.Next(lower, upper);
            }
        }

        /// <summary>
        /// Returns an int up to, but not including the parameter.  Safe for multi-threading.
        /// </summary>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static int IntLessThan(int upper)
        {
            lock (randomizer)
            {
                return randomizer.Next(upper);
            }
        }

        /// <summary>
        /// Returns a double >= 0 and less than 1.0.  Safe for multi-threading.
        /// </summary>
        /// <returns></returns>
        public static double GetDoubleFromZeroToOne()
        {
            lock (randomizer)
            {
                return randomizer.NextDouble();
            }
        }


        /// <summary>
        /// Returns a float >= 0 and less than 1.0.  Safe for multi-threading.
        /// </summary>
        /// <returns></returns>
        public static float GetFloatFromZeroToOne()
        {
            lock (randomizer)
            {
                return (float)randomizer.NextDouble();
            }
        }
    }
}
