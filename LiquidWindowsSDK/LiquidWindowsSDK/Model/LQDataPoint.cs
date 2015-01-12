using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LiquidWindowsSDK.Model
{
    public class LQDataPoint
    {
        internal LQUser _user;
        internal LQDevice _device;
        internal LQSession _session;
        internal LQEvent _event;
        internal List<LQValue> _values;
        internal DateTime _timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="LQDataPoint"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="device">The device.</param>
        /// <param name="session">The session.</param>
        /// <param name="eventObj">The event object.</param>
        /// <param name="values">The values.</param>
        /// <param name="date">The date.</param>
        public LQDataPoint(LQUser user, LQDevice device, LQSession session, LQEvent eventObj, List<LQValue> values, DateTime date)
        {
            _user = user;
            _device = device;
            _session = session;
            _event = eventObj;
            _values = values;
            _timestamp = date;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQDataPoint"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="device">The device.</param>
        /// <param name="session">The session.</param>
        /// <param name="eventObj">The event object.</param>
        /// <param name="values">The values.</param>
        public LQDataPoint(LQUser user, LQDevice device, LQSession session, LQEvent eventObj, List<LQValue> values)
            : this(user, device, session, eventObj, values, new DateTime())
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
                if (_user != null)
                {
                    var userJSON = _user.ToJson();
                    if (userJSON != null)
                    {
                        json.Add("user", userJSON);
                    }
                }

                if (_device != null)
                {
                    var deviceJSON = _device.ToJson();
                    if (deviceJSON != null)
                    {
                        json.Add("device", deviceJSON);
                    }
                }

                if (_session != null)
                {
                    var sessionJSON = _session.ToJson();
                    if (sessionJSON != null)
                    {
                        json.Add("session", sessionJSON);
                    }
                }


                if (_event != null)
                {
                    var eventJSON = _event.ToJson();
                    if (eventJSON != null)
                    {
                        json.Add("event", eventJSON);
                    }
                }

                var valuesJsonArray = new JArray();
                if (_values != null)
                {
                    foreach (LQValue value in _values)
                    {
                        var valueJSON = value.ToJson();
                        if (valueJSON != null)
                        {
                            valuesJsonArray.Add(valueJSON);
                        }
                    }
                }

                if (valuesJsonArray.Count > 0)
                {
                    json.Add("values", valuesJsonArray);
                }

                json.Add("timestamp", LiquidTools.DateToString(_timestamp));
                return json;
            }
            catch (Exception ex)
            {
                LQLog.InfoVerbose(ex.ToString());
            }
            return new JObject();
        }
    }
}
