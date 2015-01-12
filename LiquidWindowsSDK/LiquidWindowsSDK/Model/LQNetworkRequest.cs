using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace LiquidWindowsSDK.Model
{
    [DataContract(Name = "LQNetworkRequest")]
    public class LQNetworkRequest : LQModel
    {
        public const int HALF_HOUR = 30 * 60 * 1000;

        internal String USER_AGENT = "Liquid/" + Liquid.LIQUID_VERSION +
            " (Windows; Windows " + LQDevice.GetSystemVersion() + "; " + LQDevice.GetLocale() + "; " +
            (LQDevice.GetDeviceVendorAndModel() != " " ? LQDevice.GetDeviceVendorAndModel() : "Unknown")
            + ")";

        [DataMember]
        internal String _url { get; set; }
        [DataMember]
        internal String _httpMethod { get; set; }
        [DataMember]
        internal String _json { get; set; }
        [DataMember]
        internal int _numberOfTries { get; set; }
        [DataMember]
        internal DateTime? _lastTry { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="LQNetworkRequest"/> class.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <param name="httpMethod">The HTTP method to use in the request.</param>
        /// <param name="json">The json to be sent with the request.</param>
        public LQNetworkRequest(String url, String httpMethod, String json)
        {
            _url = url;
            _httpMethod = httpMethod;
            _json = json;
            _numberOfTries = 0;
            _lastTry = null;
        }

        /// <summary>
        /// Gets the URL of the request.
        /// </summary>
        /// <value>
        /// The URL of the request.
        /// </value>
        public String Url
        {
            get { return _url; }
        }

        /// <summary>
        /// Gets the HTTP method of the request.
        /// </summary>
        /// <value>
        /// The HTTP method of the request.
        /// </value>
        public String HttpMethod
        {
            get { return _httpMethod; }
        }

        /// <summary>
        /// Gets the json to be sent with the request.
        /// </summary>
        /// <value>
        /// The json to be sent with the request.
        /// </value>
        public String Json
        {
            get { return _json; }
        }

        /// <summary>
        /// Gets the number of tries the request should make before failing.
        /// </summary>
        /// <value>
        /// The number of tries the request should make before failing.
        /// </value>
        public int NumberOfTries
        {
            get { return _numberOfTries; }
        }

        /// <summary>
        /// Gets or sets when the last try was made.
        /// </summary>
        /// <value>
        /// The last try DateTime.
        /// </value>
        public DateTime? LastTry
        {
            get { return _lastTry; }
            set { _lastTry = value; }
        }

        /// <summary>
        /// Increments the number of tries made.
        /// </summary>
        public void IncrementNumberOfTries()
        {
            _numberOfTries++;
        }

        /// <summary>
        /// Will try to flush the request and set the last try value.
        /// </summary>
        /// <param name="now">The current time.</param>
        /// <returns>Whether the request will be flushed.</returns>
        public bool WillFlushAndSet(DateTime now)
        {
            bool willflush = CanFlush(now);
            if (!willflush)
            {
                _lastTry = now;
            }
            return willflush;
        }

        /// <summary>
        /// Determines whether the request can be flushed at the specified time.
        /// </summary>
        /// <param name="now">The current time.</param>
        /// <returns>Whether this request can be flushed at the specified time.</returns>
        public bool CanFlush(DateTime now)
        {
            try
            {
                return _lastTry == null || (now - _lastTry).Value.TotalMilliseconds >= HALF_HOUR;
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error verifying flush time", ex.ToString());
            }
            return false;
        }

        public override bool Equals(Object o)
        {
            bool result = false;
            try
            {
                result = (o is LQNetworkRequest) &&
                         ((LQNetworkRequest)o).HttpMethod.Equals(_httpMethod) &&
                         ((LQNetworkRequest)o).Url.Equals(_url) &&
                         ((LQNetworkRequest)o).Json.Equals(_json);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error verifying NetworkRequest Equality", ex.ToString());
            }

            return result;

        }

        public override int GetHashCode()
        {
            return (_httpMethod + _url + _json).GetHashCode();
        }

        /// <summary>
        /// Loads the requests queue.
        /// </summary>
        /// <param name="fileName">Name of the file where the requests are stored.</param>
        /// <returns></returns>
        public static async Task<List<LQNetworkRequest>> LoadQueue(String fileName)
        {
            var result = await LoadObject<List<LQNetworkRequest>>(fileName + ".queue");
            var queue = result ?? new List<LQNetworkRequest>();
            LQLog.InfoVerbose("Loading queue with " + queue.Count + " items from disk");
            return queue;
        }

        /// <summary>
        /// Saves the requests queue.
        /// </summary>
        /// <param name="queue">The queue to be stored.</param>
        /// <param name="fileName">Name of the file where the queue will be stored.</param>
        public static async Task SaveQueue(List<LQNetworkRequest> queue, String fileName)
        {
            LQLog.Data("Saving queue with " + queue.Count + " items to disk");
            await Save(fileName + ".queue", queue);
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="token">The token to authenticate the request.</param>
        /// <returns>The response to the request.</returns>
        public async Task<LQNetworkResponse> SendRequest(String token)
        {
            int responseCode = -1;
            String responseString = null;
            Stream streamResponse = null;
            StreamReader streamRead = null;

            try
            {
                var httpClient = new HttpClient();
                var httpRequestMessage = new HttpRequestMessage(new HttpMethod(_httpMethod), Url);

                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.lqd.v1+json");
                httpClient.DefaultRequestHeaders.Add("Authorization", "Token " + token);
                httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
                if (_json != null)
                {
                    var streamContent = new StringContent(_json, Encoding.UTF8, "application/json");
                    httpRequestMessage.Content = streamContent;
                }
                var response = await httpClient.SendAsync(httpRequestMessage);
                responseCode = (int)response.StatusCode;
                streamResponse = await response.Content.ReadAsStreamAsync();
                streamRead = new StreamReader(streamResponse, Encoding.UTF8);
                responseString = streamRead.ReadToEnd();
                streamResponse.Dispose();
                streamRead.Dispose();

            }
            catch (Exception ex)
            {
                LQLog.Http("Failed due to " + ex.Message + " responseCode " + responseCode);
                LQLog.Http("Error " + responseString);
            }
            finally
            {
                if (streamResponse != null)
                {
                    streamResponse.Dispose();
                }
                if (streamRead != null)
                {
                    streamRead.Dispose();
                }
            }

            if ((responseString != null) || ((responseCode >= 200) && (responseCode < 300)))
            {
                LQLog.Http("HTTP Success " + responseString);
                return new LQNetworkResponse(responseCode, responseString);
            }
            return new LQNetworkResponse(responseCode);
        }
    }
}
