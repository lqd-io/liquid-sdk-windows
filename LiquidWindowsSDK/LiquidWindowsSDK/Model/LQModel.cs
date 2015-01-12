using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace LiquidWindowsSDK.Model
{
    [DataContract(Name = "LQModel")]
    [KnownType(typeof(LQEvent))]
    [KnownType(typeof(LQSession))]
    [KnownType(typeof(LQUser))]
    [KnownType(typeof(LQNetworkRequest))]
    public abstract class LQModel
    {
        /// <summary>
        /// Generates a random unique id.
        /// </summary>
        /// <returns>The unique identifier.</returns>
        public static String NewIdentifier()
        {
            String uid = Guid.NewGuid().ToString().ToUpper();
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var timeSince1970 = (long)(DateTime.Now - epoch).TotalSeconds;
            return uid + "-" + timeSince1970.ToString(new CultureInfo("EN")).Substring(0, 10);
        }

        /// <summary>
        /// Checks if the key has invalid chars: { $ . \0 } are the invalid attributes
        /// </summary>
        /// <param name="key">The key that will be checked.</param>
        /// <param name="raiseException">If set to <c>true</c> this methd will raise an exception, otherwise will log.</param>
        /// <returns><c>true</c> if the key is valid, otherwise <c>false</c>.</returns>
        public static bool ValidKey(String key, bool raiseException)
        {
            try
            {
                bool isValid = (!key.Contains("$") && !key.Contains(".") && !key.Contains("\0"));
                if (!isValid)
                {
                    LiquidTools.ExceptionOrLog(raiseException, "Key: (" + key + ") contains invalid chars: (. $ \\0)");
                }
                return isValid;
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error validating key", ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// Checks if the attribute type is valid: {null, String, int, byte, double, float, long, short, bool, DateTime} are the valid attributes
        /// </summary>
        /// <param name="attribute">The attribute that will be checked.</param>
        /// <param name="raiseException">If set to <c>true</c> this methd will raise an exception, otherwise will log.</param>
        /// <returns><c>true</c> if the key is valid, otherwise <c>false</c>.</returns>
        public static bool ValidValue(Object attribute, bool raiseException)
        {
            try
            {
                bool isValid = ((attribute == null) || (attribute is String) ||
                                (attribute is int) || (attribute is byte) || (attribute is double) || (attribute is float) || (attribute is long) || (attribute is short) ||
                                (attribute is bool) ||
                                (attribute is DateTime));
                if (!isValid)
                {
                    LiquidTools.ExceptionOrLog(raiseException, "Key: (" + attribute + ") contains invalid chars: (. $ \\0)");
                }
                return isValid;
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error invalidating value", ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// Sanitizes the attributes.
        /// </summary>
        /// <param name="attributes">The attributes to be sanitized.</param>
        /// <param name="raiseException">If set to <c>true</c> it can raise an exception in the case of invalid attributes.</param>
        /// <returns></returns>
        public static Dictionary<String, Object> SanitizeAttributes(Dictionary<String, Object> attributes, bool raiseException)
        {
            if (attributes == null)
            {
                return null;
            }
            var attrs = new Dictionary<String, Object>();
            foreach (String key in attributes.Keys)
            {
                if (ValidKey(key, raiseException) && ValidValue(attributes[key], raiseException))
                {
                    attrs[key] = attributes[key];
                }
            }
            return attrs;
        }

        /// <summary>
        /// Saves the LQModel to the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public virtual async Task Save(String path)
        {
            LQLog.Data("Saving " + GetType().Name);
            await Save(path, this);
        }

        /// <summary>
        /// Save an LQModel based object the specified path.
        /// </summary>
        /// <typeparam name="T">The type of the LQModel Object</typeparam>
        /// <param name="path">The path.</param>
        /// <param name="obj">The object.</param>
        public static async Task Save<T>(String path, T obj)
        {
            try
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);

                var stream = new MemoryStream();
                var dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));
                dataContractJsonSerializer.WriteObject(stream, obj);
                await FileIO.WriteBufferAsync(file, stream.GetWindowsRuntimeBuffer());
            }
            catch (Exception ex)
            {
                LQLog.InfoVerbose("Could not Save to file " + path + ": " + ex);
            }
        }

        /// <summary>
        /// Loads an LQModel based object.
        /// </summary>
        /// <typeparam name="T">The type of the LQModel Object</typeparam>
        /// <param name="path">The path where the object is saved.</param>
        /// <returns>The LQModel based object.</returns>
        public static async Task<T> LoadObject<T>(String path)
        {
            try
            {
                LQLog.InfoVerbose("Loading " + path + " from disk");

                var dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));

                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile sampleFile = await folder.GetFileAsync(path);
                var result = await FileIO.ReadBufferAsync(sampleFile);

                return (T)dataContractJsonSerializer.ReadObject(result.AsStream());

            }
            catch (Exception ex)
            {
                LQLog.InfoVerbose("Could not Load liquid package from file: " + ex);
            }

            return default(T);
        }

        /// <summary>
        /// Loads an LQModel based object stored in the specified path.
        /// </summary>
        /// <typeparam name="T">The type of the LQModel Object</typeparam>
        /// <param name="path">The path where the object is saved.</param>
        /// <returns>The LQModel based object.</returns>
        public static async Task<T> Load<T>(String path)
        {
            return await LoadObject<T>(path);
        }
    }
}
