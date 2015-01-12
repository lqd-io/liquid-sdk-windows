using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiquidWindowsSDK.Model
{
    class LQLiquidPackage
    {
        internal const String LIQUID_PACKAGE_FILENAME = "LiquidPackage";
        internal List<LQValue> _values = new List<LQValue>();
        internal static SemaphoreSlim _fileLock = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="LQLiquidPackage"/> class.
        /// </summary>
        public LQLiquidPackage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQLiquidPackage"/> class with the json information of it.
        /// </summary>
        /// <param name="jsonObject">The json object where the LQLiquidPackage information is stored.</param>
        public LQLiquidPackage(JObject jsonObject)
        {
            try
            {
                var valuesJsonArray = jsonObject["values"] as JArray;
                if (valuesJsonArray != null)
                {
                    foreach (JToken token in valuesJsonArray)
                    {
                        var valueJson = token as JObject;
                        var v = new LQValue(valueJson);
                        _values.Add(v);
                    }
                }
            }
            catch (Exception ex)
            {
                LQLog.Error("Parsing LQLiquidPackage: " + ex.Message);
            }

        }

        /// <summary>
        /// Determines whether the LQLiquidPackage is empty.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return (_values.Count == 0);
        }

        /// <summary>
        /// Gets the values of the package.
        /// </summary>
        /// <value>
        /// The values of the package.
        /// </value>
        public List<LQValue> Values
        {
            get
            {
                return _values;
            }
            internal set
            {
                _values = value;
            }
        }

        /// <summary>
        /// Invalidates the target from the variable key.
        /// </summary>
        /// <param name="variableKey">The variable key.</param>
        /// <returns>Whether the target was invalidated.</returns>
        public bool InvalidateTargetFromVariableKey(String variableKey)
        {
            bool invalidated = false;
            foreach (LQValue value in _values)
            {
                if (value.Variable.Name.Equals(variableKey))
                {
                    try
                    {
                        invalidated = value.TargetId == null ? InvalidateValue(variableKey) : InvalidateValuesOfTarget(value.TargetId);
                    }
                    catch (Exception ex)
                    {
                        LiquidTools.LogUnexpectedException("Unexpected error invalidating variable", ex.ToString());
                    }
                }
            }
            return invalidated;
        }

        /// <summary>
        /// Invalidates the values of a target.
        /// </summary>
        /// <param name="targetId">The target identifier.</param>
        /// <returns>Whether the values were invalidated.</returns>
        internal bool InvalidateValuesOfTarget(String targetId)
        {
            int valueItems = _values.Count;
            var tempvar = new List<LQValue>();

            foreach (LQValue value in _values)
            {
                if (!targetId.Equals(value.TargetId))
                {
                    tempvar.Add(value);
                    --valueItems;
                }
            }
            _values = tempvar;
            return valueItems > 0;
        }

        /// <summary>
        /// Invalidates a value.
        /// </summary>
        /// <param name="variableKey">The variable key of the value.</param>
        /// <returns>Whether the value was invalidated.</returns>
        internal bool InvalidateValue(String variableKey)
        {
            int valueItems = _values.Count;
            var tempvar = new List<LQValue>();

            foreach (LQValue value in _values)
            {
                try
                {
                    if (!value.Variable.Name.Equals(variableKey))
                    {
                        tempvar.Add(value);
                        --valueItems;
                    }
                }
                catch (Exception ex)
                {
                    LiquidTools.LogUnexpectedException("Unexpected error invalidating variable", ex.ToString());
                }
            }
            _values = tempvar;
            return valueItems > 0;
        }

        /// <summary>
        /// Saves to this instance to disk.
        /// </summary>
        public async Task SaveToDisk()
        {
            LQLog.Data("Saving to local storage");
            try
            {
                await _fileLock.WaitAsync();
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile file =
                    await
                        folder.CreateFileAsync(LIQUID_PACKAGE_FILENAME + ".vars",
                            CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(this));
            }
            catch (Exception ex)
            {
                LQLog.InfoVerbose("Could not save liquid package to file: " + ex);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <summary>
        /// Loads from this instance from disk.
        /// </summary>
        /// <returns>The LQLiquidPackage loaded from disk.</returns>
        public static async Task<LQLiquidPackage> LoadFromDisk()
        {
            LQLog.Data("Loading from local storage");
            bool error = false;
            StorageFile sampleFile = null;
            try
            {
                await _fileLock.WaitAsync();
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                sampleFile = await folder.GetFileAsync(LIQUID_PACKAGE_FILENAME + ".vars");

                var result = await FileIO.ReadTextAsync(sampleFile);

                return JsonConvert.DeserializeObject<LQLiquidPackage>(result) ?? new LQLiquidPackage();
            }
            catch (FileNotFoundException)
            {
                LQLog.InfoVerbose("Could not Load liquid package from file. File does not exist.");
            }
            catch (Exception)
            {
                error = true;
                LQLog.InfoVerbose("Could not Load liquid package from file.");
            }
            finally
            {
                _fileLock.Release();
            }

            if (error)
            {
                if (sampleFile != null)
                {
                    try
                    {
                        await _fileLock.WaitAsync();
                        await sampleFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                    catch (Exception)
                    {
                        LQLog.InfoVerbose("Error deleting liquid package file after exception.");
                    }
                    finally
                    {
                        _fileLock.Release();
                    }
                }
            }

            return new LQLiquidPackage();
        }
    }
}
