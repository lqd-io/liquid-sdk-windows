using System;

namespace LiquidWindowsSDK
{
    public class UniqueTime
    {
        internal static int _increment = 1;

        /// <summary>
        /// Creates a unique DateTime instance.
        /// </summary>
        /// <returns>A new DateTime object.</returns>
        public static DateTime NewDate()
        {
            DateTime cal = DateTime.Now;
            try
            {
                cal = cal.AddMilliseconds(_increment);
                _increment = (_increment + 1) % 200;
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error creating new date", ex.ToString());
            }
            return cal;
        }
    }
}
