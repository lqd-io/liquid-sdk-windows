using System;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI;

namespace LiquidWindowsSDK
{
    public class LiquidTools
    {
        /// <summary>
        /// Determines whether Network is available.
        /// </summary>
        /// <returns>Whether the network is available.</returns>
        public static bool IsNetworkAvailable()
        {
            bool connected = false;
            var profile = NetworkInformation.GetInternetConnectionProfile();

            try
            {
                var connectivityLevel = profile.GetNetworkConnectivityLevel();

                if (connectivityLevel == NetworkConnectivityLevel.InternetAccess)
                    connected = true;
            }
            catch (Exception ex)
            {
                LQLog.InfoVerbose(ex.ToString());
            }

            return connected;
        }

        /// <summary>
        /// Outputs a DateTime object into a String formatted using the ISO8601 specification.
        /// </summary>
        /// <param name="date">The date to be outputed.</param>
        /// <returns>The string representation of the date using the ISO8601 specification.</returns>
        public static String DateToString(DateTime date)
        {
            return ISO8601Utils.Format(date, true);
        }

        /// <summary>
        /// Strings to date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static DateTime StringToDate(String date)
        {
            var parsedDate = ISO8601Utils.Parse(date);
            return ConvertToCurrentTimeZone(parsedDate);
        }

        /// <summary>
        /// Converts a DateTime object to the current time zone.
        /// </summary>
        /// <param name="value">The DateTime to Convert.</param>
        /// <returns>A dateTime in The Current TimeZone.</returns>
        public static DateTime ConvertToCurrentTimeZone(DateTime value)
        {
            return value.ToLocalTime();
        }

        /// <summary>
        /// Transforms a Color object to an hexadecimal string of the format #xxxxxxxx or #xxxxxx.
        /// </summary>
        /// <param name="color">The color to be converted.</param>
        /// <returns>The string corresponding to the Color object inputed.</returns>
        public static String ColorToHex(Color color)
        {
            return ColorToHex(color, false);
        }

        /// <summary>
        /// Transforms a Color object to an hexadecimal string of the format #xxxxxxxx or #xxxxxx.
        /// </summary>
        /// <param name="color">The color to be converted.</param>
        /// <param name="useAlphaValue">If set to <c>true</c> it will include the alpha value of the color.</param>
        /// <returns>The string corresponding to the Color object inputed.</returns>
        public static String ColorToHex(Color color, bool useAlphaValue)
        {
            try
            {
                return useAlphaValue ?
                    String.Format("#{0}{1}{2}{3}", color.A.ToString("X2"), color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2")) :
                    String.Format("#{0}{1}{2}", color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error converting color to hex", ex.ToString());
            }

            return string.Empty;
        }

        /// <summary>
        /// Transforms a color in hexadecimal format #xxxxxxxx to a Color object.
        /// </summary>
        /// <param name="value">The color value in the hexadecimal format #xxxxxxxx.</param>
        /// <returns>The color object corresponding to the input string.</returns>
        public static Color HexToColor(String value)
        {
            try
            {
                byte alpha;
                byte pos = 0;

                string hex = value.Replace("#", "");

                if (hex.Length == 8)
                {
                    alpha = Convert.ToByte(hex.Substring(pos, 2), 16);
                    pos = 2;
                }
                else
                {
                    alpha = Convert.ToByte("ff", 16);
                }

                byte red = Convert.ToByte(hex.Substring(pos, 2), 16);

                pos += 2;
                byte green = Convert.ToByte(hex.Substring(pos, 2), 16);

                pos += 2;
                byte blue = Convert.ToByte(hex.Substring(pos, 2), 16);

                return Color.FromArgb(alpha, red, green, blue);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error converting hex to color", ex.ToString());
            }
            return new Color();
        }

        /// <summary>
        /// Saves a setting to the disk.
        /// </summary>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        public static void SaveToDisk(String key, String value)
        {
            try
            {
                var romaingSettings = ApplicationData.Current.RoamingSettings;
                romaingSettings.Values.Add("io.lqd." + key, value);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error converting saving to disk", ex.ToString());
            }
        }

        /// <summary>
        /// Loads a setting from the disk.
        /// </summary>
        /// <param name="key">The key of the setting to be loaded.</param>
        /// <returns>The string value of the setting.</returns>
        public static String LoadFromDisk(String key)
        {
            try
            {
                var romaingSettings = ApplicationData.Current.RoamingSettings;
                return romaingSettings.Values["io.lqd." + key] as string;
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error converting loading from disk", ex.ToString());
            }
            return string.Empty;
        }

        /// <summary>
        /// Logs an error or raises an exception
        /// </summary>
        /// <param name="raiseException">If set to <c>true</c> it will raise an exception.</param>
        /// <param name="message">The message to be logged.</param>
        public static void ExceptionOrLog(bool raiseException, String message)
        {
            if (raiseException)
            {
                throw new Exception(message);
            }
            else
            {
                LQLog.Warning(message);
            }
        }

        /// <summary>
        /// Logs an error or raises an exception
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exceptionInfo">The information from the exception</param>
        public static void LogUnexpectedException(String message, String exceptionInfo)
        {
            if (Liquid.Instance != null && Liquid.Instance._isDevelopmentMode)
            {
                throw new Exception(message + ": " + exceptionInfo);
            }
            else
            {
                LQLog.Error(message + ": " + exceptionInfo);
            }
        }
    }
}
