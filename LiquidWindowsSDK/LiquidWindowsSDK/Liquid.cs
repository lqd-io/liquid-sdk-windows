using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using LiquidWindowsSDK.Model;
using Newtonsoft.Json.Linq;

namespace LiquidWindowsSDK
{
    public class Liquid
    {
        public const String TAG_LIQUID = "LIQUID";

        public const String LIQUID_VERSION = "0.1.2-beta";
        internal const int LIQUID_DEFAULT_SESSION_TIMEOUT = 30; //seconds

        internal static int _sessionTimeout;
        internal String _apiToken;
        internal LQUser _currentUser;
        internal LQUser _previousUser;
        internal LQDevice _device;
        internal LQSession _currentSession;
        internal DateTime? _enterBackgroundtime;
        internal bool _autoLoadValues;
        internal static Liquid _instance;
        internal LQLiquidPackage _loadedLiquidPackage;
        internal Dictionary<String, LQValue> _appliedValues = new Dictionary<String, LQValue>();
        internal List<String> _bundleVariablesSent;
        internal LQQueuer _httpQueuer;
        internal bool _isDevelopmentMode;

        private static object _receivedLock = new object();
        public static event EventHandler ValuesReceived;
        protected static void OnValuesReceived()
        {
            EventHandler handler = ValuesReceived;
            if (handler != null)
            {
                handler(null, null);
            }
        }

        public static event EventHandler ValuesLoaded;
        protected static void OnValuesLoaded()
        {
            EventHandler handler = ValuesLoaded;
            if (handler != null)
            {
                handler(null, null);
            }
        }

        internal Liquid()
        {
        }

        /// <summary>
        /// 
        /// Retrieves the Liquid shared instance.
        /// <p>
        /// You can use this method across all your activities.
        /// </p>
        /// 
        /// </summary>
        /// <value>
        /// The Liquid instance.
        /// </value>
        public static Liquid Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception("Can't call Instance before Initialize()");
                }
                else
                {
                    return _instance;
                }
            }
        }

        /// <summary>
        /// By default Liquid will not auto load variables.
        /// Gets or sets Liquid behavior to auto load variables.
        /// </summary>
        /// <value>
        /// <c>true</c> if variables be automatically loaded otherwise, <c>false</c>.
        /// </value>
        public bool AutoLoadValues
        {
            get { return _autoLoadValues; }
            set { _autoLoadValues = value; }
        }

        /// <summary>
        /// Gets or sets the timeout value that Liquid is using to close automatically a session.
        /// </summary>
        /// <value>
        /// In seconds the value of the timeout.
        /// </value>
        public int SessionTimeout
        {
            get { return _sessionTimeout; }
            set { _sessionTimeout = value; }
        }

        /// <summary>
        /// Call this method to initialize Liquid.
        /// </summary>
        /// <param name="apiToken">The Liquid ApiToken of your app.</param>
        /// <param name="application">The application instance.</param>
        /// <param name="args">The previous application state arguments.</param>
        /// <returns>The Liquid instance.</returns>
        public static async Task<Liquid> Initialize(String apiToken, Application application, LaunchActivatedEventArgs args)
        {
            return await Initialize(apiToken, application, args, false);
        }


        /// <summary>
        /// Call this method to initialize Liquid.
        /// </summary>
        /// <param name="apiToken">The Liquid ApiToken of your app.</param>
        /// <param name="application">The Windows application instance.</param>
        /// <param name="args">The previous application state arguments.</param>
        /// <param name="developmentMode">The flag to send to Liquid server the variables used in methods with <b>fallbackVariable</b> param.</param>
        /// <returns>The Liquid instance.</returns>
        public static async Task<Liquid> Initialize(string apiToken, Application application, LaunchActivatedEventArgs args, bool developmentMode)
        {
            if (_instance != null)
            {
                return _instance;
            }
            else
            {
                _instance = new Liquid();
                await _instance.InitializeLiquid(apiToken, application, args, developmentMode);
                return _instance;
            }
        }

        /// <summary>
        /// Initializes a Liquid instance.
        /// </summary>
        /// <param name="apiToken">The API token.</param>
        /// <param name="application">The application instance.</param>
        /// <param name="args">The previous application state arguments.</param>
        /// <param name="developmentMode">if set to <c>true</c> this instance is in development mode.</param>
        /// <returns>The Liquid instance.</returns>
        internal async Task InitializeLiquid(string apiToken, Application application, LaunchActivatedEventArgs args, bool developmentMode)
        {
            application.Resuming += ApplicationOnResuming;
            application.Suspending += ApplicationOnSuspending;

            if (string.IsNullOrEmpty(apiToken))
            {
                throw new Exception("Your API Token is invalid: \'" + apiToken + "\'.");
            }

            try
            {
                _sessionTimeout = LIQUID_DEFAULT_SESSION_TIMEOUT;
                _apiToken = apiToken;
                _device = new LQDevice(LIQUID_VERSION);
                _loadedLiquidPackage = await LQLiquidPackage.LoadFromDisk();
                _appliedValues = LQValue.ConvertToDictionary(_loadedLiquidPackage.Values);

                _httpQueuer = new LQQueuer(_apiToken, await LQNetworkRequest.LoadQueue(_apiToken));
                _httpQueuer.StartFlushTimer();
                _isDevelopmentMode = developmentMode;
                if (_isDevelopmentMode)
                {
                    _bundleVariablesSent = new List<String>();
                }

                // Get last user and init session
                _previousUser = await LQUser.Load(_apiToken);
                await IdentifyUser(_previousUser.Identifier, _previousUser.Attributes, _previousUser.Identified, false);

                ApplicationLauchedOrResumedCallback(args);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error initializing Liquid", ex.ToString());
            }

            LQLog.Info("Initialized Liquid with API Token " + apiToken);
        }

        internal async void ApplicationOnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        {
            var deferral = suspendingEventArgs.SuspendingOperation.GetDeferral();
            await ApplicationSuspendedCallback();
            deferral.Complete();
        }

        internal void ApplicationOnResuming(object sender, object o)
        {
            ApplicationLauchedOrResumedCallback(null);
        }

        /// <summary>
        /// Sets the flush interval.
        /// </summary>
        /// <param name="flushInterval">The flush interval value in seconds.</param>
        public void SetFlushInterval(int flushInterval)
        {
            _httpQueuer.SetFlushTimer(flushInterval);
        }

        /// <summary>
        /// Gets the flush interval.
        /// </summary>
        /// <returns>The flush interval value in seconds.</returns>
        public int GetFlushInterval()
        {
            return _httpQueuer.FlushTimer;
        }

        /// <summary>
        /// Setups the push notifications.
        /// </summary>
        public async Task SetupPushNotifications()
        {
            //Load Settings URI imediately
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("channelUri"))
            {
                Instance.SetWNSDeviceUri(ApplicationData.Current.LocalSettings.Values["channelUri"].ToString());
            }
            await LQPushHandler.CreateNotificationChannel();
        }

        /// <summary>
        /// Creates an Alias for the current user.
        /// </summary>
        public async Task Alias()
        {
            String oldID = _previousUser.Identifier;
            String newID = _currentUser.Identifier;
            if (_previousUser.Identified)
            {
                LQLog.Warning("Can't alias (" + oldID + "): Isn't an anonymous user.");
                return;
            }
            LQLog.InfoVerbose("Making alias between (" + oldID + ") and (" + newID + ").");
            await SingleThreadExecutor.RunAsync(async () =>
            {
                var aliasRequest = LQRequestFactory.CreateAliasRequest(oldID, newID);
                if (aliasRequest != null)
                {
                    await aliasRequest.SendRequest(_apiToken);
                }
            });
        }

        /// <summary>
        /// Resets the user information.
        /// </summary>
        public async Task ResetUser()
        {
            String automaticIdentifier = LQModel.NewIdentifier();
            await IdentifyUser(automaticIdentifier, null, false, false);
        }

        /// <summary>
        /// Identifies the current user with a generated UUID.
        /// </summary>
        /// <param name="identifier">The custom UUID.</param>
        public async Task IdentifyUser(String identifier)
        {
            await IdentifyUser(identifier, null, true, true);
        }

        /// <summary>
        /// Identifies the current user with a generated UUID.
        /// </summary>
        /// <param name="identifier">The custom UUID.</param>
        /// <param name="alias">If set to <c>true</c>, will make an alias with the previous user if the previous user is anonymous.</param>
        public async Task IdentifyUser(String identifier, bool alias)
        {
            await IdentifyUser(identifier, null, true, alias);
        }

        /// <summary>
        /// Identifies the current user with a generated UUID.
        /// </summary>
        /// <param name="identifier">The custom UUID.</param>
        /// <param name="attributes">Additional user attributes.</param>
        public async Task IdentifyUser(String identifier, Dictionary<String, Object> attributes)
        {
            await IdentifyUser(identifier, attributes, true, true);
        }

        /// <summary>
        /// Identifies the current user with a generated UUID.
        /// </summary>
        /// <param name="identifier">The custom UUID.</param>
        /// <param name="attributes">Additional user attributes.</param>
        /// <param name="alias">If set to <c>true</c>, will make an alias with the previous user if the previous user is anonymous.</param>
        public async Task IdentifyUser(String identifier, Dictionary<String, Object> attributes, bool alias)
        {
            await IdentifyUser(identifier, attributes, true, alias);
        }

        internal async Task IdentifyUser(String identifier, Dictionary<String, Object> attributes, bool identified, bool alias)
        {
            String finalIdentifier = identifier;
            Dictionary<String, Object> finalAttributes = LQModel.SanitizeAttributes(attributes, _isDevelopmentMode);

            if (string.IsNullOrEmpty(identifier))
            {
                return;
            }

            if (_currentUser != null && _currentUser.Identifier.Equals(identifier))
            {
                try
                {
                    _currentUser.SetAttributes(finalAttributes);
                    await _currentUser.Save(_apiToken);
                    LQLog.InfoVerbose("Already identified with user " + finalIdentifier + ". Not identifying again.");
                }
                catch (Exception ex)
                {
                    LiquidTools.LogUnexpectedException("Unexpected error checking user", ex.ToString());
                }
                return;
            }

            try
            {
                DestroySession(UniqueTime.NewDate());

                _previousUser = _currentUser;
                _currentUser = new LQUser(finalIdentifier, finalAttributes, identified);
                NewSession(true);
                await RequestValues(false);
                await _currentUser.Save(_apiToken);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error identifying user", ex.ToString());
            }

            if (alias)
            {
                await Alias();
            }

            LQLog.Info("From now on we're identifying the User by the identifier '" + finalIdentifier + "'");
        }

        /// <summary>
        /// Gets the user UUID
        /// </summary>
        /// <returns>The user UUID, null if the user isn't identified.</returns>
        public String GetUserIdentifier()
        {
            if (_currentUser == null)
            {
                return null;
            }
            return _currentUser.Identifier;
        }

        /// <summary>
        /// Gets the session UUID
        /// </summary>
        /// <returns>The session UUID, null if the session isn't identified.</returns>
        public String GetSessionIdentifier()
        {
            if (_currentSession == null)
            {
                return null;
            }
            return _currentSession.Id;
        }

        /// <summary>
        /// Adds or updates an additional attribute to the user.
        /// </summary>
        /// <param name="key">The attribute key.</param>
        /// <param name="attribute">The attribute value.</param>
        public async Task SetUserAttribute(String key, Object attribute)
        {
            if (LQModel.ValidKey(key, _isDevelopmentMode))
            {
                String finalKey = key;
                Object finalAttribute = attribute;
                await SingleThreadExecutor.RunAsync(async () =>
                {
                    _currentUser.SetAttribute(finalKey, finalAttribute);
                    await _currentUser.Save(_apiToken);
                });
            }
        }

        /// <summary>
        /// Adds or updates the current location.
        /// </summary>
        /// <param name="location">The current location.</param>
        public async Task SetCurrentLocation(Geocoordinate location)
        {
            await SingleThreadExecutor.RunAsync(async () =>
            {
                _device.SetLocation(location);
                await _currentUser.Save(_apiToken);
            });
        }

        /// <summary>
        /// Adds or updates the WNS device URI.
        /// </summary>
        /// <param name="uri">The WNS device URI.</param>
        public void SetWNSDeviceUri(String uri)
        {
            _device.SetPushId(uri);
        }

        /// <summary>
        /// Removes the WNS device URI.
        /// </summary>
        public void RemoveWNSdeviceUri()
        {
            _device.SetPushId(null);
        }

        internal async void NewSession(bool runInCurrentThread)
        {
            DateTime now = UniqueTime.NewDate();
            LQLog.InfoVerbose("Open Session: " + now);

            if (runInCurrentThread)
            {
                _currentSession = new LQSession(_sessionTimeout, now);
                Track("_startSession", null, now);
            }
            else
            {
                await SingleThreadExecutor.RunAsync(() =>
                {
                    _currentSession = new LQSession(_sessionTimeout, now);
                    Track("_startSession", null, now);
                });
            }

        }

        /// <summary>
        /// Closes the current session and opens a new one.
        /// </summary>
        public void DestroySession()
        {
            DestroySession(UniqueTime.NewDate());
            NewSession(false);
        }

        internal void DestroySession(DateTime closeDate)
        {
            if ((_currentUser != null) && (_currentSession != null)
                    && _currentSession.End == null)
            {
                LQLog.InfoVerbose("Close Session: " + closeDate.ToString());
                _currentSession.End = closeDate;
                Track("_endSession", null, closeDate);
            }
        }

        /// <summary>
        /// Tracks an event.
        ///
        /// <p>
        /// If the <b>eventName</b> is a empty string or null, the event will be
        /// tracked with name <b>unnamedEvent</b>
        /// </p>
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        public void Track(String eventName)
        {
            if (LQEvent.HasValidName(eventName, _isDevelopmentMode))
            {
                Track(eventName, null, UniqueTime.NewDate());
            }
            else
            {
                LQLog.Warning("Event can't begin with \' _ \' character ");
            }
        }

        /// <summary>
        /// Tracks an event.
        ///
        /// <p>
        /// If the <b>eventName</b> is a empty string or null, the event will be
        /// tracked with name <b>unnamedEvent</b>
        /// </p>
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="attributes">Additional attributes of the event.</param>
        public void Track(String eventName, Dictionary<String, Object> attributes)
        {
            if (LQEvent.HasValidName(eventName, _isDevelopmentMode))
            {
                Track(eventName, attributes, UniqueTime.NewDate());
            }
        }

        internal async void Track(String eventName, Dictionary<String, Object> attributes, DateTime date)
        {
            try
            {
                if (string.IsNullOrEmpty(eventName))
                {
                    eventName = "unnamedEvent";
                }
                LQLog.InfoVerbose("Tracking: " + eventName);

                var lqevent = new LQEvent(eventName, LQModel.SanitizeAttributes(attributes, _isDevelopmentMode), date);
                var values = _loadedLiquidPackage == null ? null : _loadedLiquidPackage.Values;
                String datapoint = new LQDataPoint(_currentUser, _device, _currentSession, lqevent, values, date).ToJson().ToString();
                LQLog.Data(datapoint);

                await SingleThreadExecutor.RunAsync(() => _httpQueuer.AddToHttpQueue(LQRequestFactory.CreateDataPointRequest(datapoint)));
            }
            catch (Exception ex)
            {
                LQLog.InfoVerbose("Error tracking event: " + ex);
            }
        }

        internal async Task ApplicationSuspendedCallback()
        {
            try
            {
                Track("_pauseSession", null, UniqueTime.NewDate());
                _enterBackgroundtime = UniqueTime.NewDate();
                await Flush();
                await RequestValues();
                await LoadValues();
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error suspending application", ex.ToString());
            }
        }

        internal void ApplicationLauchedOrResumedCallback(LaunchActivatedEventArgs args)
        {
            try
            {
                if (args != null && args.PreviousExecutionState.Equals(ApplicationExecutionState.ClosedByUser))
                {
                    DestroySession((_enterBackgroundtime ?? DateTime.Today));
                    NewSession(false);
                }
                else
                {
                    if ((_currentSession != null) && (_enterBackgroundtime != null))
                    {
                        DateTime now = UniqueTime.NewDate();
                        double interval = (now - ((DateTime)_enterBackgroundtime)).TotalSeconds;
                        if (interval >= _sessionTimeout)
                        {
                            DestroySession(((DateTime)_enterBackgroundtime));
                            NewSession(false);
                        }
                        else
                        {
                            Track("_resumeSession", null, UniqueTime.NewDate());
                        }
                    }
                    else
                    {
                        //Track("_startSession", null, UniqueTime.NewDate());
                    }
                }
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error resuming application", ex.ToString());
            }
        }

        /// <summary>
        /// Requests values from the server.
        /// </summary>
        public async Task RequestValues(bool runInCurrentThread = true)
        {
            if ((_currentUser != null) && (_device != null))
            {
                if (runInCurrentThread)
                {
                    await RequestValuesWork();
                }
                else
                {
                    await SingleThreadExecutor.RunAsync(async () =>
                    {
                        await RequestValuesWork();
                    });
                }

            }
        }

        private async Task RequestValuesWork()
        {
            LQNetworkRequest req = LQRequestFactory.RequestLiquidPackageRequest(_currentUser.Identifier, _device.Uid);
            var sendRequest = (await req.SendRequest(_apiToken));
            String dataFromServer = null;
            if (sendRequest != null)
            {
                dataFromServer = sendRequest.RequestResponse;
            }
            if (dataFromServer != null)
            {
                try
                {
                    var jsonObject = JObject.Parse(dataFromServer);
                    var liquidPackage = new LQLiquidPackage(jsonObject);
                    LQLog.Http(jsonObject.ToString());
                    await liquidPackage.SaveToDisk();
                }
                catch (Exception)
                {
                    LQLog.Error("Could not parse JSON " + dataFromServer);
                }
                NotifyListeners(true);
                if (_autoLoadValues)
                {
                    await LoadLiquidPackage(true);
                }
            }
        }

        internal void NotifyListeners(bool received)
        {
            if (received)
            {
                OnValuesReceived();
            }
            else
            {
                OnValuesLoaded();
            }
        }

        /// <summary>
        /// Loads the values retrieved previously from the server.
        /// </summary>
        public async Task LoadValues()
        {
            await LoadLiquidPackage(true);
        }

        internal async Task LoadLiquidPackage(bool runInCurrentThread)
        {
            if (runInCurrentThread)
            {
                await LoadLiquidPackageWork();
            }
            else
            {
                await SingleThreadExecutor.RunAsync(async () =>
                {
                    await LoadLiquidPackageWork();
                });
            }
        }

        private async Task LoadLiquidPackageWork()
        {
            try
            {
                _loadedLiquidPackage = await LQLiquidPackage.LoadFromDisk();
                _appliedValues = LQValue.ConvertToDictionary(_loadedLiquidPackage.Values);
                NotifyListeners(false);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error loading Liquid Package", ex.ToString());
            }
        }

        /// <summary>
        /// Gets a DateTime variable value.
        /// </summary>
        /// <param name="variableKey">The variable key of the value.</param>
        /// <param name="fallbackValue">The the value returned if the value for variableKey doesn't exist in the Liquid instance.</param>
        /// <returns>The value in Liquid instance or the fallbackValue if the variable doesn't exist.</returns>
        public async Task<DateTime?> GetDateVariable(String variableKey, DateTime fallbackValue)
        {
            try
            {
                if (_isDevelopmentMode)
                {
                    await SendBundleVariable(LQVariable.BuildJsonObject(variableKey, fallbackValue, LQVariable.DATE_TYPE));
                }
                if (!_appliedValues.ContainsKey(variableKey))
                {
                    return fallbackValue;
                }
                if (_appliedValues[variableKey].GetDataType().Equals(LQVariable.DATE_TYPE))
                {
                    try
                    {
                        var value = _appliedValues[variableKey].Value as DateTime?;
                        return value == null ? (DateTime?)null : LiquidTools.ConvertToCurrentTimeZone((DateTime)value);
                    }
                    catch (Exception)
                    {
                        LQLog.Error("Error parsing Date with key: \"" + variableKey + "\"");
                    }
                }
                InvalidateVariable(variableKey);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting date variable", ex.ToString());
            }
            return fallbackValue;
        }

        /// <summary>
        /// Gets a Color variable value.
        /// </summary>
        /// <param name="variableKey">The variable key of the value.</param>
        /// <param name="fallbackValue">The the value returned if the value for variableKey doesn't exist in the Liquid instance.</param>
        /// <returns>The value in Liquid instance or the fallbackValue if the variable doesn't exist.</returns>
        public async Task<Color> GetColorVariable(String variableKey, Color fallbackValue)
        {
            try
            {
                if (_isDevelopmentMode)
                {
                    await SendBundleVariable(LQVariable.BuildJsonObject(variableKey, LiquidTools.ColorToHex(fallbackValue), LQVariable.COLOR_TYPE));
                }
                if (!_appliedValues.ContainsKey(variableKey))
                {
                    return fallbackValue;
                }
                if (_appliedValues[variableKey].GetDataType().Equals(LQVariable.COLOR_TYPE))
                {
                    try
                    {
                        return LiquidTools.HexToColor(_appliedValues[variableKey].Value.ToString());
                    }
                    catch (Exception)
                    {
                        LQLog.Error("Error parsing Color with key: \"" + variableKey + "\"");
                    }
                }
                InvalidateVariable(variableKey);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting color variable", ex.ToString());
            }
            return fallbackValue;
        }

        /// <summary>
        /// Gets a String variable value.
        /// </summary>
        /// <param name="variableKey">The variable key of the value.</param>
        /// <param name="fallbackValue">The the value returned if the value for variableKey doesn't exist in the Liquid instance.</param>
        /// <returns>The value in Liquid instance or the fallbackValue if the variable doesn't exist.</returns>
        public async Task<String> GetStringVariable(String variableKey, String fallbackValue)
        {
            try
            {
                if (_isDevelopmentMode)
                {
                    await SendBundleVariable(LQVariable.BuildJsonObject(variableKey, fallbackValue, LQVariable.STRING_TYPE));
                }
                if (!_appliedValues.ContainsKey(variableKey))
                {
                    return fallbackValue;
                }
                if (_appliedValues[variableKey].GetDataType().Equals(LQVariable.STRING_TYPE))
                {
                    Object value = _appliedValues[variableKey].Value;
                    return value == null ? null : value.ToString();
                }
                InvalidateVariable(variableKey);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting string variable", ex.ToString());
            }
            return fallbackValue;
        }

        /// <summary>
        /// Gets an Integer variable value.
        /// </summary>
        /// <param name="variableKey">The variable key of the value.</param>
        /// <param name="fallbackValue">The the value returned if the value for variableKey doesn't exist in the Liquid instance.</param>
        /// <returns>The value in Liquid instance or the fallbackValue if the variable doesn't exist.</returns>
        public async Task<int> GetIntVariable(String variableKey, int fallbackValue)
        {
            try
            {
                if (_isDevelopmentMode)
                {
                    await SendBundleVariable(LQVariable.BuildJsonObject(variableKey, fallbackValue, LQVariable.INT_TYPE));
                }
                if (!_appliedValues.ContainsKey(variableKey))
                {
                    return fallbackValue;
                }
                if (_appliedValues[variableKey].GetDataType().Equals(LQVariable.INT_TYPE))
                {
                    try
                    {
                        return int.Parse(_appliedValues[variableKey].Value.ToString());
                    }
                    catch (Exception)
                    {
                        LQLog.Error("Error parsing Integer with key: \"" + variableKey + "\"");
                    }
                }
                InvalidateVariable(variableKey);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting int variable", ex.ToString());
            }
            return fallbackValue;
        }

        /// <summary>
        /// Gets a Float variable value.
        /// </summary>
        /// <param name="variableKey">The variable key of the value.</param>
        /// <param name="fallbackValue">The the value returned if the value for variableKey doesn't exist in the Liquid instance.</param>
        /// <returns>The value in Liquid instance or the fallbackValue if the variable doesn't exist.</returns>
        public async Task<float> GetFloatVariable(String variableKey, float fallbackValue)
        {
            try
            {
                if (_isDevelopmentMode)
                {
                    await SendBundleVariable(LQVariable.BuildJsonObject(variableKey, fallbackValue, LQVariable.FLOAT_TYPE));
                }
                if (!_appliedValues.ContainsKey(variableKey))
                {
                    return fallbackValue;
                }
                if (_appliedValues[variableKey].GetDataType().Equals(LQVariable.FLOAT_TYPE))
                {
                    try
                    {
                        return float.Parse(_appliedValues[variableKey].Value.ToString());
                    }
                    catch (Exception)
                    {
                        LQLog.Error("Error parsing Float with key: \"" + variableKey + "\"");
                    }
                }
                InvalidateVariable(variableKey);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting float variable", ex.ToString());
            }
            return fallbackValue;
        }

        /// <summary>
        /// Gets a Boolean variable value.
        /// </summary>
        /// <param name="variableKey">The variable key of the value.</param>
        /// <param name="fallbackValue">The the value returned if the value for variableKey doesn't exist in the Liquid instance.</param>
        /// <returns>The value in Liquid instance or the fallbackValue if the variable doesn't exist.</returns>
        public async Task<bool> GetBooleanVariable(String variableKey, bool fallbackValue)
        {
            try
            {
                if (_isDevelopmentMode)
                {
                    await SendBundleVariable(LQVariable.BuildJsonObject(variableKey, fallbackValue, LQVariable.BOOLEAN_TYPE));
                }
                if (!_appliedValues.ContainsKey(variableKey))
                {
                    return fallbackValue;
                }
                if (_appliedValues[variableKey].GetDataType().Equals(LQVariable.BOOLEAN_TYPE))
                {
                    return bool.Parse(_appliedValues[variableKey].Value.ToString());
                }

                InvalidateVariable(variableKey);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting boolean variable", ex.ToString());
            }
            return fallbackValue;

        }

        /// <summary>
        /// Forces Liquid to send locally saved data.
        /// </summary>
        public async Task Flush()
        {
            LQLog.InfoVerbose("Flushing");
            await SingleThreadExecutor.RunAsync(async () =>
            {
                if (_httpQueuer != null)
                {
                    await _httpQueuer.Flush();
                }
            });
        }

        internal async Task SendBundleVariable(JObject variable)
        {
            if (!_bundleVariablesSent.Contains(variable["name"] == null ? string.Empty : variable["name"].ToString()))
            {
                await SingleThreadExecutor.RunAsync(async () =>
                {
                    LQLog.InfoVerbose("Sending bundle variable " + variable);
                    await LQRequestFactory.CreateVariableRequest(variable).SendRequest(_apiToken);
                });

                _bundleVariablesSent.Add(variable["name"] == null ? string.Empty : variable["name"].ToString());
            }
        }

        /// <summary>
        ///  Reset all collected data that is stored locally.
        ///
        /// <p>
        /// This includes, user, device, session, values, events
        /// </p>
        /// </summary>
        public async Task Reset()
        {
            await Reset(false);
        }

        /// <summary>
        /// Same as reset but preserves the tracked events that aren't in Liquid Server.
        /// </summary>
        public async Task SoftReset()
        {
            await Reset(true);
        }

        internal async Task Reset(bool soft)
        {
            await SingleThreadExecutor.RunAsync(async () =>
            {
                try
                {
                    _currentSession = null;
                    _device = new LQDevice(LIQUID_VERSION);
                    _enterBackgroundtime = null;
                    _loadedLiquidPackage = new LQLiquidPackage();
                    _appliedValues = new Dictionary<String, LQValue>();
                    if (!soft)
                    {
                        _httpQueuer = new LQQueuer(_apiToken);
                    }
                    await ResetUser();
                }
                catch (Exception ex)
                {
                    LiquidTools.LogUnexpectedException("Unexpected error reseting user", ex.ToString());
                }
            });

        }

        internal async void InvalidateVariable(String variableKey)
        {
            try
            {
                LQLog.InfoVerbose("invalidating: " + variableKey);
                bool removed = _loadedLiquidPackage.InvalidateTargetFromVariableKey(variableKey);
                if (removed)
                {
                    LQLog.InfoVerbose("invalidated: " + variableKey);
                    _appliedValues = LQValue.ConvertToDictionary(_loadedLiquidPackage.Values);
                    await _loadedLiquidPackage.SaveToDisk();
                    NotifyListeners(false);
                }
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error invalidating variable", ex.ToString());
            }
        }

    }
}
