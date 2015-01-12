using System;

namespace LiquidWindowsSDK.Model
{
    public class LQNetworkResponse
    {
        internal int _httpCode;
        internal String _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="LQNetworkResponse"/> class.
        /// </summary>
        public LQNetworkResponse()
            : this(-1)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQNetworkResponse"/> class.
        /// </summary>
        /// <param name="httpCode">The HTTP code of the network response.</param>
        public LQNetworkResponse(int httpCode)
            : this(httpCode, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQNetworkResponse"/> class.
        /// </summary>
        /// <param name="httpCode">The HTTP code of the response.</param>
        /// <param name="response">The response string.</param>
        public LQNetworkResponse(int httpCode, String response)
        {
            _httpCode = httpCode;
            _data = response;
        }

        /// <summary>
        /// Gets the HTTP code of the response.
        /// </summary>
        /// <value>
        /// The HTTP code of the response.
        /// </value>
        public int HttpCode
        {
            get { return _httpCode; }
        }

        /// <summary>
        /// Gets the request response.
        /// </summary>
        /// <value>
        /// The request response.
        /// </value>
        public String RequestResponse
        {
            get { return _data; }
        }

        /// <summary>
        /// Determines whether this response has succeeded.
        /// </summary>
        /// <returns></returns>
        public bool HasSucceeded()
        {
            return _httpCode >= 200 && _httpCode < 300;
        }

        /// <summary>
        /// Determines whether this respose was forbidden.
        /// </summary>
        /// <returns></returns>
        public bool HasForbidden()
        {
            return _httpCode == 401 || _httpCode == 403;
        }
    }
}
