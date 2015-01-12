using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LiquidWindowsSDK.Model
{

    public class LQValue
    {
        internal String _id;
        internal Object _value;
        internal LQVariable _variable;
        internal bool _isDefault;
        internal String _targetId;

        /// <summary>
        /// Initializes a new instance of the <see cref="LQValue"/> class.
        /// </summary>
        public LQValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQValue"/> class.
        /// </summary>
        /// <param name="jsonObject">The json object with the LQValue information.</param>
        public LQValue(JObject jsonObject)
        {
            try
            {
                _id = jsonObject.Value<String>("id");
                _value = jsonObject.GetValue("value");
                if (jsonObject.GetValue("target_id") != null)
                {
                    _targetId = jsonObject.Value<String>("target_id");
                }

                var v = jsonObject.GetValue("variable") as JObject;
                _variable = new LQVariable(v);
            }
            catch (Exception ex)
            {
                LQLog.Error("Parsing LQValue: " + ex.Message);
            }
        }

        /// <summary>
        /// Gets or sets the LQValue identifier.
        /// </summary>
        /// <value>
        /// The LQValue identifier.
        /// </value>
        public String Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public Object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Gets or sets the variable.
        /// </summary>
        /// <value>
        /// The variable.
        /// </value>
        public LQVariable Variable
        {
            get { return _variable; }
            set { _variable = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this value is the default.
        /// </summary>
        /// <value>
        /// <c>true</c> if this value is the default; otherwise, <c>false</c>.
        /// </value>
        public bool IsDefault
        {
            get { return _isDefault; }
            set { _isDefault = value; }
        }

        /// <summary>
        /// Gets or sets the target identifier.
        /// </summary>
        /// <value>
        /// The target identifier.
        /// </value>
        public String TargetId
        {
            get { return _targetId; }
            set { _targetId = value; }
        }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public String GetDataType()
        {
            return _variable.DataType;
        }

        public override bool Equals(Object o)
        {
            return o is LQValue && _id.Equals(((LQValue)o)._id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        /// <summary>
        /// Builds the json object of the current LQValue.
        /// </summary>
        /// <returns>A JObject that represents the current LQValue.</returns>
        public JObject ToJson()
        {
            var json = new JObject();
            try
            {
                json.Add("id", _id);
                json.Add("target_id", _targetId);
                return json;
            }
            catch (Exception ex)
            {
                LQLog.Error("LQVALUE ToJson:" + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Converts a list of LQValues to a dictionary.
        /// </summary>
        /// <param name="values">The LQValues.</param>
        /// <returns>The dictionary corresponding the the LQValues inputted</returns>
        public static Dictionary<String, LQValue> ConvertToDictionary(List<LQValue> values)
        {
            var dictionary = new Dictionary<String, LQValue>();
            foreach (LQValue value in values)
            {
                if (value.Value != null)
                {
                    if (value.Variable.Name != null)
                    {
                        dictionary[value.Variable.Name] = value;
                    }
                }
            }
            return dictionary;
        }

        /// <summary>
        /// Returns a string that represents the current an LQVariable object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return "LQValue [_identifier=" + _id + ", _value=" + _value
                    + ", _variable=" + _variable + ", _isDefault=" + _isDefault
                    + "]";
        }
    }
}
