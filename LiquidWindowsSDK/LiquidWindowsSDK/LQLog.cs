using System;
using System.Diagnostics;

namespace LiquidWindowsSDK
{
    public class LQLog
    {
        public static int LOG_LEVEL = 2;

        internal const int PATHS = 7;
        internal const int HTTP = 6;
        internal const int DATA = 5;
        internal const int INFO_VERBOSE = 4;
        internal const int INFO = 3;
        internal const int WARNING = 2;
        internal const int ERROR = 1;

        /// <summary>
        /// Logs a path message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Paths(String message)
        {
            if (LOG_LEVEL >= PATHS)
            {
                Debug.WriteLine(Liquid.TAG_LIQUID + ": " + message);
            }
        }

        /// <summary>
        /// Logs a http message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Http(String message)
        {
            if (LOG_LEVEL >= HTTP)
            {
                Debug.WriteLine(Liquid.TAG_LIQUID + ": " + message);
            }
        }

        /// <summary>
        /// Logs a data message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Data(String message)
        {
            if (LOG_LEVEL >= DATA)
            {
                Debug.WriteLine(Liquid.TAG_LIQUID + ": " + message);
            }
        }

        /// <summary>
        /// Logs an information message set as verbose.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void InfoVerbose(String message)
        {
            if (LOG_LEVEL >= INFO_VERBOSE)
            {
                Debug.WriteLine(Liquid.TAG_LIQUID + ": " + message);
            }
        }

        /// <summary>
        /// Logs an warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Warning(String message)
        {
            if (LOG_LEVEL >= WARNING)
            {
                Debug.WriteLine(Liquid.TAG_LIQUID + ": " + message);
            }
        }

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Info(String message)
        {
            if (LOG_LEVEL >= INFO)
            {
                Debug.WriteLine(Liquid.TAG_LIQUID + ": " + message);
            }
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Error(String message)
        {
            if (LOG_LEVEL >= ERROR)
            {
                Debug.WriteLine(Liquid.TAG_LIQUID + ": " + message);
            }
        }
    }
}
