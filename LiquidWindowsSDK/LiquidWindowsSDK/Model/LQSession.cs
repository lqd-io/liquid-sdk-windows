using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace LiquidWindowsSDK.Model
{
    [DataContract(Name = "LQSession")]
    public class LQSession : LQModel
    {
        [DataMember]
        internal String _id { get; set; }
        [DataMember]
        internal DateTime? _end { get; set; }
        [DataMember]
        internal DateTime _start { get; set; }
        [DataMember]
        internal int _timeout { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQSession"/> class.
        /// </summary>
        /// <param name="timeout">The timeout value for this session.</param>
        /// <param name="date">The start date of the session.</param>
        public LQSession(int timeout, DateTime date)
        {
            _id = NewIdentifier();
            _timeout = timeout;
            _end = null;
            _start = date;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQSession"/> class.
        /// </summary>
        /// <param name="timeout">The timeout value for this session.</param>
        public LQSession(int timeout)
            : this(timeout, new DateTime())
        {

        }

        /// <summary>
        /// Gets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public String Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the start time of the session.
        /// </summary>
        /// <value>
        /// The start time of the session.
        /// </value>
        public DateTime Start
        {
            get { return _start; }
        }

        /// <summary>
        /// Gets or sets the end time of the session.
        /// </summary>
        /// <value>
        /// The end time of the session.
        /// </value>
        public DateTime? End
        {
            get { return _end; }
            set { _end = value; }
        }

        /// <summary>
        /// Builds the json object of the current LQSession.
        /// </summary>
        /// <returns>A JObject that represents the current LQSession.</returns>
        public JObject ToJson()
        {
            var json = new JObject();
            try
            {
                json.Add("started_at", LiquidTools.DateToString(_start));
                json.Add("timeout", _timeout);
                json.Add("unique_id", _id);
                if (_end != null)
                {
                    json.Add("ended_at", LiquidTools.DateToString((DateTime)_end));
                }
                return json;
            }
            catch (Exception ex)
            {
                LQLog.Error("LQSession ToJson: " + ex.Message);
            }
            return null;
        }
    }
}
