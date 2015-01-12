using System;
using Newtonsoft.Json.Linq;

namespace LiquidWindowsSDK.Model
{
    public class LQVariable
    {
        public const String DATE_TYPE = "datetime";
        public const String STRING_TYPE = "string";
        public const String INT_TYPE = "integer";
        public const String FLOAT_TYPE = "float";
        public const String COLOR_TYPE = "color";
        public const String BOOLEAN_TYPE = "boolean";

        internal String _id;
        internal String _name;
        internal String _dataType;
        internal String _targetId;

        /// <summary>
        /// Initializes a new instance of the <see cref="LQVariable"/> class.
        /// </summary>
        public LQVariable()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQVariable"/> class.
        /// </summary>
        /// <param name="jsonObject">The json object with the LQVariable information.</param>
        public LQVariable(JObject jsonObject)
        {
            try
            {
                _id = (String)jsonObject.GetValue("id");
                _name = (String)jsonObject.GetValue("name");
                _targetId = jsonObject.Value<String>("target_id") ?? string.Empty;
                _dataType = (String)jsonObject.GetValue("data_type");
            }
            catch (Exception ex)
            {
                LQLog.Error("Parsing LQVariable: " + ex.Message);
            }
        }

        /// <summary>
        /// Gets or sets the variable identifier.
        /// </summary>
        /// <value>
        /// The variable identifier.
        /// </value>
        public String Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// Gets or sets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the data type of the variable.
        /// </summary>
        /// <value>
        /// The data type of the variable.
        /// </value>
        public String DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        /// <summary>
        /// Gets or sets the target identifier.
        /// </summary>
        /// <value>
        /// The target identifier.
        /// </value>
        internal String TargetId
        {
            get { return _targetId; }
            set { _targetId = value; }
        }

        /// <summary>
        /// Builds the json object of the current LQVariable.
        /// </summary>
        /// <returns>A JObject that represents the current LQVariable.</returns>
        public JObject ToJson()
        {
            var json = new JObject();
            try
            {
                json.Add("id", _id);
                json.Add("name", _name);
                json.Add("data_type", _dataType);
                return json;
            }
            catch (Exception ex)
            {
                LQLog.Error("LQVariable ToJson: " + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Builds the json object of an LQVariable.
        /// </summary>
        /// <param name="variableKey">The variable key.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <returns>A JObject that represents the LQVariable inputted.</returns>
        internal static JObject BuildJsonObject(String variableKey, String variableType)
        {
            var variable = new JObject { { "name", variableKey }, { "data_type", variableType } };
            return variable;
        }

        /// <summary>
        /// Builds the json object of an LQVariable.
        /// </summary>
        /// <param name="variableKey">The variable key.</param>
        /// <param name="variableValue">The variable value.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <returns>A JObject that represents the LQVariable inputted.</returns>
        public static JObject BuildJsonObject(String variableKey, String variableValue, String variableType)
        {
            JObject variable = null;
            try
            {
                variable = BuildJsonObject(variableKey, variableType);
                variable.Add("default_value", variableValue);
            }
            catch (Exception ex)
            {
                LQLog.Error("LQVariable buildJSON: " + ex.Message);
            }
            return variable;
        }

        /// <summary>
        /// Builds the json object of an LQVariable.
        /// </summary>
        /// <param name="variableKey">The variable key.</param>
        /// <param name="variableValue">The variable value.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <returns>A JObject that represents the LQVariable inputted.</returns>
        public static JObject BuildJsonObject(String variableKey, int variableValue, String variableType)
        {
            JObject variable = null;
            try
            {
                variable = BuildJsonObject(variableKey, variableType);
                variable.Add("default_value", variableValue);
            }
            catch (Exception ex)
            {
                LQLog.Error("LQVariable buildJSON: " + ex.Message);
            }
            return variable;
        }

        /// <summary>
        /// Builds the json object of an LQVariable.
        /// </summary>
        /// <param name="variableKey">The variable key.</param>
        /// <param name="variableValue">The variable value.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <returns>A JObject that represents the LQVariable inputted.</returns>
        public static JObject BuildJsonObject(String variableKey, bool variableValue, String variableType)
        {
            JObject variable = null;
            try
            {
                variable = BuildJsonObject(variableKey, variableType);
                variable.Add("default_value", variableValue);
            }
            catch (Exception ex)
            {
                LQLog.Error("LQVariable buildJSON: " + ex.Message);
            }
            return variable;
        }

        /// <summary>
        /// Builds the json object of an LQVariable.
        /// </summary>
        /// <param name="variableKey">The variable key.</param>
        /// <param name="variableValue">The variable value.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <returns>A JObject that represents the LQVariable inputted.</returns>
        public static JObject BuildJsonObject(String variableKey, float variableValue, String variableType)
        {
            JObject variable = null;
            try
            {
                variable = BuildJsonObject(variableKey, variableType);
                variable.Add("default_value", variableValue);
            }
            catch (Exception ex)
            {
                LQLog.Error("LQVariable buildJSON: " + ex.Message);
            }
            return variable;
        }

        /// <summary>
        /// Builds the json object of an LQVariable.
        /// </summary>
        /// <param name="variableKey">The variable key.</param>
        /// <param name="variableValue">The variable value.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <returns>A JObject that represents the LQVariable inputted.</returns>
        public static JObject BuildJsonObject(String variableKey, DateTime variableValue, String variableType)
        {
            JObject variable = null;
            try
            {
                variable = BuildJsonObject(variableKey, variableType);
                variable.Add("default_value", LiquidTools.DateToString(variableValue));
            }
            catch (Exception ex)
            {
                LQLog.Error("LQVariable buildJSON: " + ex.Message);
            }
            return variable;
        }


        /// <summary>
        /// Returns a string that represents the current an LQVariable object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override String ToString()
        {
            return "LQVariable [_id=" + _id + ", _name=" + _name
                    + ", mType=" + _dataType + "]";
        }
    }
}
