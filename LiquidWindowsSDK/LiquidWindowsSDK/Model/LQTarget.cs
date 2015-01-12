using System;
using Newtonsoft.Json.Linq;

namespace LiquidWindowsSDK.Model
{
    public class LQTarget
    {
        internal String _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="LQTarget"/> class.
        /// </summary>
        /// <param name="id">The target identifier.</param>
        public LQTarget(String id)
        {
            _id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQTarget"/> class.
        /// </summary>
        /// <param name="jsonObject">The json object with the LQTarget information.</param>
        public LQTarget(JObject jsonObject)
        {
            try
            {
                _id = jsonObject.Value<String>("id");
            }
            catch (Exception ex)
            {
                LQLog.Error("Parsing LQTarget: " + ex.Message);
            }
        }

        /// <summary>
        /// Gets the target identifier.
        /// </summary>
        /// <value>
        /// The target identifier.
        /// </value>
        public String Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Builds the json object of the current LQTarget.
        /// </summary>
        /// <returns>A JObject that represents the current LQTarget.</returns>
        public JObject ToJson()
        {
            var json = new JObject();
            try
            {
                json.Add("id", _id);
                return json;
            }
            catch (Exception ex)
            {
                LQLog.Error("LQTarget ToJson: " + ex.Message);
            }
            return null;
        }
    }
}
