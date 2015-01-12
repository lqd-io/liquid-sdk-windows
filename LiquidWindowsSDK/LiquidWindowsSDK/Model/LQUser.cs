using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Data.Json;
using Newtonsoft.Json.Linq;

namespace LiquidWindowsSDK.Model
{
    [DataContract(Name = "LQUser")]
    public class LQUser : LQModel
    {
        [DataMember]
        internal String _identifier { get; set; }
        [DataMember]
        internal bool _identified { get; set; }
        [DataMember]
        internal Dictionary<String, Object> _attributes { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="LQUser"/> class.
        /// </summary>
        /// <param name="identifier">The user identifier.</param>
        public LQUser(String identifier)
            : this(identifier, new Dictionary<String, Object>())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQUser"/> class.
        /// </summary>
        /// <param name="identifier">The user identifier.</param>
        /// <param name="identified">If this is set to <c>true</c> the user is identified.</param>
        public LQUser(String identifier, bool identified)
            : this(identifier, new Dictionary<String, Object>(), identified)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQUser"/> class.
        /// </summary>
        /// <param name="identifier">The user identifier.</param>
        /// <param name="attributes">The user attributes.</param>
        public LQUser(String identifier, Dictionary<String, Object> attributes)
            : this(identifier, attributes, true)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQUser"/> class.
        /// </summary>
        /// <param name="identifier">The user identifier.</param>
        /// <param name="attributes">The user attributes.</param>
        /// <param name="identified">If this is set to <c>true</c> the user is identified.</param>
        public LQUser(String identifier, Dictionary<String, Object> attributes, bool identified)
        {
            _identifier = identifier;
            _attributes = attributes;
            AttributesCheck();
            _identified = identified;
        }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public String Identifier
        {
            get { return _identifier; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LQUser"/> is identified.
        /// </summary>
        /// <value>
        ///   <c>true</c> if identified; otherwise, <c>false</c>.
        /// </value>
        public bool Identified
        {
            get { return _identified; }
            set { _identified = value; }
        }

        public override bool Equals(Object o)
        {
            return (o is LQUser) && (ToJson().ToString().Equals(((LQUser)o).ToJson().ToString()));
        }

        public override int GetHashCode()
        {
            return ToJson().ToString().GetHashCode();
        }

        /// <summary>
        /// Gets the user attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Dictionary<String, Object> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        /// <summary>
        /// Sets the user attributes.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        public void SetAttributes(Dictionary<String, Object> attributes)
        {
            _attributes = attributes;
            AttributesCheck();
        }

        /// <summary>
        /// Sets an attribute to the current LQUser.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="attribute">The attribute.</param>
        public void SetAttribute(String key, Object attribute)
        {
            _attributes[key] = attribute;
        }

        /// <summary>
        /// Gets the attribute corresponding to the key given.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The attribute of the inputted key.</returns>
        public Object AttributeForKey(String key)
        {
            return _attributes[key];
        }

        /// <summary>
        /// Builds the json object of the current LQUser.
        /// </summary>
        /// <returns>A JObject that represents the current LQUser.</returns>
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
                json.Add("unique_id", _identifier);
                json.Add("identified", _identified);
                return json;
            }
            catch (Exception ex)
            {
                LQLog.Error("LQUser ToJson: " + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Check the attributes of the current LQUser
        /// </summary>
        internal void AttributesCheck()
        {
            if (_attributes == null)
            {
                _attributes = new Dictionary<String, Object>();
            }
        }

        /// <summary>
        /// Saves the current LQUser in the specified path.
        /// </summary>
        /// <param name="path">The path to store the LQUser information.</param>
        public override async Task Save(String path)
        {
            await base.Save(path + ".user");
        }

        /// <summary>
        /// Loads the user information in the specified path.
        /// </summary>
        /// <param name="path">The path where the information is stored.</param>
        /// <returns>The LQUser object.</returns>
        public static async Task<LQUser> Load(String path)
        {
            var user = await Load<LQUser>(path + ".user") ?? new LQUser(NewIdentifier(), false);
            user.AttributesCheck();
            return user;
        }
    }
}
