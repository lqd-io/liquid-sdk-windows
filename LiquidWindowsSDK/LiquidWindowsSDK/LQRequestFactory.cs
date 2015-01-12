using System;
using LiquidWindowsSDK.Model;
using Newtonsoft.Json.Linq;

namespace LiquidWindowsSDK
{
    public class LQRequestFactory
    {
        internal const String LIQUID_SERVER_BASE_URL = "https://api.lqd.io/collect/";
        internal const String LIQUID_DATAPOINT_URL = LIQUID_SERVER_BASE_URL + "data_points";
        internal const String LIQUID_ALIAS_URL = LIQUID_SERVER_BASE_URL + "aliases";
        internal const String LIQUID_LQD_PACKAGE_URL = LIQUID_SERVER_BASE_URL + "users/{0}/devices/{1}/liquid_package";
        internal const String LIQUID_VARIABLES_URL = LIQUID_SERVER_BASE_URL + "variables";

        /// <summary>
        /// Creates a new Alias Request.
        /// </summary>
        /// <param name="oldId">The old identifier.</param>
        /// <param name="newId">The new identifier.</param>
        /// <returns>The Network Request to create a new Alias.</returns>
        public static LQNetworkRequest CreateAliasRequest(String oldId, String newId)
        {
            try
            {
                var json = new JObject { { "unique_id", newId }, { "unique_id_alias", oldId } };
                return new LQNetworkRequest(LIQUID_ALIAS_URL, "POST", json.ToString());
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a DataPoint request.
        /// </summary>
        /// <param name="dataPoint">The Datapoint upon which the request will be created.</param>
        /// <returns>The Datapoint Network Request.</returns>
        public static LQNetworkRequest CreateDataPointRequest(String dataPoint)
        {
            return new LQNetworkRequest(LIQUID_DATAPOINT_URL, "POST", dataPoint);
        }

        /// <summary>
        /// Creates a request for a Liquid Package
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userDevice">The user device unique identifier.</param>
        /// <returns>The Liquid Package Network Request</returns>
        public static LQNetworkRequest RequestLiquidPackageRequest(String userId, String userDevice)
        {
            String url = String.Format(LIQUID_LQD_PACKAGE_URL, userId, userDevice);
            return new LQNetworkRequest(url, "GET", null);
        }

        /// <summary>
        /// Creates a variable request.
        /// </summary>
        /// <param name="variable">The variable upon which the request will be created.</param>
        /// <returns>The variable Network Request.</returns>
        public static LQNetworkRequest CreateVariableRequest(JObject variable)
        {
            return new LQNetworkRequest(LIQUID_VARIABLES_URL, "POST", variable.ToString());
        }
    }
}
