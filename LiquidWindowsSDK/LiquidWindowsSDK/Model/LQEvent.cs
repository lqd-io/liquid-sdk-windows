using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace LiquidWindowsSDK.Model
{
    [DataContract(Name = "LQEvent")]
    public class LQEvent : LQModel
    {
        [DataMember]
        internal String _name { get; set; }
        [DataMember]
        internal Dictionary<String, Object> _attributes { get; set; }
        [DataMember]
        internal DateTime _date { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQEvent"/> class.
        /// </summary>
        /// <param name="name">The name of the event.</param>
        /// <param name="attributes">The attributes of the event.</param>
        /// <param name="date">The time when the event occured.</param>
        public LQEvent(String name, Dictionary<String, Object> attributes, DateTime date)
        {
            _name = name;
            _attributes = attributes ?? new Dictionary<String, Object>();
            _date = date;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQEvent"/> class.
        /// </summary>
        /// <param name="name">The name of the event.</param>
        /// <param name="attributes">The attributes of the event.</param>
        public LQEvent(String name, Dictionary<String, Object> attributes)
            : this(name, attributes, new DateTime())
        {

        }

        /// <summary>
        /// Builds the json object of the current LQEvent.
        /// </summary>
        /// <returns>A JObject that represents the current LQEvent.</returns>
        public JObject ToJson()
        {
            var json = new JObject();
            try
            {
                if (_attributes != null)
                {
                    foreach (String key in _attributes.Keys)
                    {
                        if (_attributes[key] is DateTime)
                        {
                            json.Add(key, LiquidTools.DateToString((DateTime)_attributes[key]));
                        }
                        else
                        {
                            json.Add(new JProperty(key, _attributes[key]));
                        }
                    }
                }
                json.Add("name", _name);
                json.Add("date", LiquidTools.DateToString(_date));
                return json;
            }
            catch (Exception ex)
            {
                LQLog.Error("LQEvent ToJson: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Determines whether the event has a valid name.
        /// </summary>
        /// <param name="name">The name of the event.</param>
        /// <param name="raiseException">If set to <c>true</c> it will raise an exception if the event has an invalid name.</param>
        /// <returns></returns>
        public static bool HasValidName(String name, bool raiseException)
        {
            bool isValid = string.IsNullOrEmpty(name) || name[0] != '_';
            if (!isValid)
            {
                LiquidTools.ExceptionOrLog(raiseException, "Event can't begin with \' _ \' character ");
            }
            return isValid;
        }
    }
}
