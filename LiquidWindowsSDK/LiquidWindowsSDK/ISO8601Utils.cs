using System;

namespace LiquidWindowsSDK
{
    public static class ISO8601Utils
    {
        /// <summary>
        /// Format a date into 'yyyy-MM-ddThh:mm:ssZ' (no milliseconds precision)
        /// </summary>
        /// <param name="date">The date to format.</param>
        /// <returns>The date formatted as 'yyyy-MM-ddThh:mm:sszzz'.</returns>
        public static String Format(DateTime date)
        {
            return Format(date, false);
        }

        /// <summary>
        /// Format a date into 'yyyy-MM-ddThh:mm:ss[.fff]zzz'
        /// </summary>
        /// <param name="date">The date to format.</param>
        /// <param name="millis"><c>true</c> to include millis precision, otherwise <c>false.</c></param>
        /// <returns>The date formatted as 'yyyy-MM-ddThh:mm:ss[.fff]zzz'.</returns>
        public static String Format(DateTime date, bool millis)
        {
            try
            {
                if (millis)
                {
                    return date.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
                }
                else
                {
                    return date.ToString("yyyy-MM-ddTHH:mm:sszzz");
                }
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error formatting date", ex.ToString());
            }
            return string.Empty;
        }

        /// <summary>
        /// Parses a date from ISO-8601 formatted string. It expects a format yyyy-MM-ddThh:mm:ss[.sss][zzz|
        /// </summary>
        /// <param name="date">ISO string to parse in the appropriate format.</param>
        /// <returns>The parsed date.</returns>
        public static DateTime Parse(String date)
        {
            var parsed = new DateTime();
            try
            {
                parsed = DateTime.Parse(date);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error parsing date", ex.ToString());
            }

            return parsed;
        }
    }
}
