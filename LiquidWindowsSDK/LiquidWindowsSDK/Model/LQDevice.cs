using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Devices.Geolocation;
using Windows.Graphics.Display;
using Windows.Networking.Connectivity;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Newtonsoft.Json.Linq;

namespace LiquidWindowsSDK.Model
{
    public class LQDevice
    {
        internal String _vendor;
        internal String _deviceModel;
        internal String _deviceName;
        internal String _systemVersion;
        internal String _screenSize;
        internal String _carrier;
        internal String _internetConnectivity;

        internal String _uid;
        internal String _appBundle;
        internal String _appVersion;
        internal String _appName;
        internal String _liquidVersion;
        internal Dictionary<String, Object> _attributes;

        internal String _locale;
        internal String _systemLanguage;

        internal static EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();

        /// <summary>
        /// Initializes a new instance of the <see cref="LQDevice"/> class.
        /// </summary>
        /// <param name="liquidVersion">The liquid version.</param>
        public LQDevice(String liquidVersion)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    _attributes = new Dictionary<String, Object>();

                    _vendor = GetDeviceVendor();
                    _deviceModel = GetDeviceModel();
                    _deviceName = GetDeviceName();
                    _systemVersion = GetSystemVersion();
                    _screenSize = GetScreenSize();
                    _carrier = GetCarrier();
                    _internetConnectivity = GetInternetConnectivity();
                    _uid = GetDeviceID();
                    _appBundle = GetAppBundle();
                    _appName = await GetAppName();
                    _appVersion = GetAppVersion();
                    _liquidVersion = liquidVersion;
                    _locale = GetLocale();
                    _systemLanguage = GetSystemLanguage();
                }
                catch (Exception ex)
                {
                    LiquidTools.LogUnexpectedException("Unexpected error getting device properties", ex.ToString());
                }
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQDevice"/> class.
        /// </summary>
        /// <param name="liquidVersion">The liquid version.</param>
        /// <param name="location">The location of the device.</param>
        public LQDevice(String liquidVersion, Geocoordinate location)
            : this(liquidVersion)
        {
            SetLocation(location);
        }

        /// <summary>
        /// Gets the uid of the device.
        /// </summary>
        /// <value>
        /// The uid of the device.
        /// </value>
        public String Uid
        {
            get { return _uid; }
        }

        /// <summary>
        /// Sets the location of the device.
        /// </summary>
        /// <param name="location">The location of the device.</param>
        public void SetLocation(Geocoordinate location)
        {
            try
            {
                if (location == null)
                {
                    _attributes.Remove("latitude");
                    _attributes.Remove("longitude");
                }
                else
                {
                    _attributes["latitude"] = location.Point.Position.Latitude;
                    _attributes["longitude"] = location.Point.Position.Longitude;
                }
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error setting position", ex.ToString());
            }
        }

        /// <summary>
        /// Sets the push identifier for the device.
        /// </summary>
        /// <param name="id">The push identifier for the device.</param>
        public void SetPushId(String id)
        {
            try
            {
                if (id == null || id.Equals(""))
                {
                    _attributes.Remove("push_token");
                }
                else
                {
                    _attributes["push_token"] = id;
                }
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error setting push id", ex.ToString());
            }
        }

        /// <summary>
        /// Builds the json object of the current LQDevice.
        /// </summary>
        /// <returns>A JObject that represents the current LQDevice.</returns>
        public JObject ToJson()
        {
            _internetConnectivity = GetInternetConnectivity();

            var attrs = new Dictionary<String, Object>();

            try
            {
                if (_attributes != null)
                {
                    foreach (var attribute in _attributes)
                    {
                        attrs[attribute.Key] = attribute.Value;
                    }
                }
                attrs["vendor"] = _vendor;
                attrs["platform"] = "Windows";
                attrs["model"] = _deviceModel;

                int version;
                if (int.TryParse(_systemVersion, out version))
                {
                    attrs["system_version"] = version;
                }
                else
                {
                    attrs["system_version"] = _systemVersion;
                }

                attrs["screen_size"] = _screenSize;
                attrs["carrier"] = _carrier;
                attrs["internet_connectivity"] = _internetConnectivity;
                attrs["unique_id"] = _uid;
                attrs["app_bundle"] = _appBundle;
                attrs["app_name"] = _appName;
                attrs["app_version"] = _appVersion;
                attrs["liquid_version"] = _liquidVersion;
                attrs["locale"] = _locale;
                attrs["system_language"] = _systemLanguage;
                attrs["name"] = _deviceName;
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error converting device attributes to JSON", ex.ToString());
            }

            var json = new JObject();
            try
            {
                foreach (String key in attrs.Keys)
                {
                    if (attrs[key] is DateTime)
                    {
                        json.Add(key, LiquidTools.DateToString((DateTime)attrs[key]));
                    }
                    else
                    {
                        json.Add(new JProperty(key, attrs[key]));
                    }
                }
                return json;
            }
            catch (Exception ex)
            {
                LQLog.Error("LQDevice ToJson: " + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Gets the device vendor.
        /// </summary>
        /// <returns>The device vendor.</returns>
        internal static String GetDeviceVendor()
        {
            if (deviceInfo != null)
            {
                return deviceInfo.SystemManufacturer;
            }
            else
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// Gets the device model.
        /// </summary>
        /// <returns>The device model.</returns>
        internal static String GetDeviceModel()
        {
            if (deviceInfo != null)
            {
                return deviceInfo.SystemProductName;
            }
            else
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// Gets the device vendor.
        /// </summary>
        /// <returns>The device vendor.</returns>
        internal static String GetDeviceVendorAndModel()
        {
            if (deviceInfo != null)
            {
                return deviceInfo.SystemManufacturer + " " + deviceInfo.SystemProductName;
            }
            else
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// Gets the device name.
        /// </summary>
        /// <returns>The device name.</returns>
        internal static String GetDeviceName()
        {
            if (deviceInfo != null)
            {
                return deviceInfo.FriendlyName;
            }
            else
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// Gets the device system version.
        /// </summary>
        /// <returns>The device system version.</returns>
        internal static String GetSystemVersion()
        {
            if (deviceInfo != null)
            {
                // we'll keep this hardcoded until a new release of system version because Microsoft doesn't deliver a proper way to get it
                return "8.1";
            }
            else
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// Gets the device system language.
        /// </summary>
        /// <returns>The device system language.</returns>
        internal static String GetSystemLanguage()
        {
            try
            {
                return Windows.System.UserProfile.GlobalizationPreferences.Languages[0];
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting System Language", ex.ToString());
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the current locale.
        /// </summary>
        /// <returns>The device current locale.</returns>
        internal static String GetLocale()
        {
            try
            {
                return Windows.System.UserProfile.GlobalizationPreferences.HomeGeographicRegion;
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting locale", ex.ToString());
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the screen resolution of the device.
        /// </summary>
        /// <returns>The screen resolution of the device.</returns>
        internal static String GetScreenSize()
        {
            //TODO This is correct for Windows 8.1 but not for Windows Phone 8.1
            double width = 0;
            double height = 0;

            try
            {
#if WINDOWS_PHONE_APP

            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;

            width = Window.Current.Bounds.Width * scaleFactor;
            height = Window.Current.Bounds.Height * scaleFactor;
#else
                width = Window.Current.Bounds.Width;
                height = Window.Current.Bounds.Height;
#endif
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting screen size", ex.ToString());
            }

            return Math.Round(width) + "x" + Math.Round(height);
        }

        /// <summary>
        /// Gets the carrier of the device.
        /// </summary>
        /// <returns>The carrier being used on the device.</returns>
        internal static String GetCarrier()
        {
            try
            {
                var result = NetworkInformation.GetConnectionProfiles();

                foreach (var connectionProfile in result)
                {
                    if (connectionProfile.IsWwanConnectionProfile)
                    {
                        foreach (var networkName in connectionProfile.GetNetworkNames())
                        {
                            ApplicationData.Current.LocalSettings.Values["carrier"] = networkName;
                            return networkName;
                        }
                    }

                }

                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("carrier"))
                {
                    return ApplicationData.Current.LocalSettings.Values["carrier"].ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting Carrier", ex.ToString());
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the type internet connectivity.
        /// </summary>
        /// <returns>A string representing the type of the internet connectivity.</returns>
        internal static String GetInternetConnectivity()
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (InternetConnectionProfile == null)
            {
                return "No Connectivity";
            }
            try
            {
                var interfaceType = InternetConnectionProfile.NetworkAdapter.IanaInterfaceType;
                if (interfaceType == 6)
                {
                    return "Ethernet";
                }
                else if (interfaceType == 71)
                {
                    return "WiFi";
                }
                else
                {
                    return "Cellular";
                }
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting internet connectivity", ex.ToString());
            }
            return "No Connectivity";
        }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <returns>The device identifier.</returns>
        public static String GetDeviceID()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("DeviceUID"))
            {
                return ApplicationData.Current.LocalSettings.Values["DeviceUID"].ToString();
            }
            else
            {
                var token = HardwareIdentification.GetPackageSpecificToken(null);
                var hardwareId = token.Id;
                var dataReader = DataReader.FromBuffer(hardwareId);

                var bytes = new byte[hardwareId.Length];
                dataReader.ReadBytes(bytes);

                string deviceUid = BitConverter.ToString(bytes);

                if (deviceUid.Length < 90)
                {
                    var generatedUid = LQModel.NewIdentifier();
                    ApplicationData.Current.LocalSettings.Values.Add("DeviceUID", generatedUid);
                    return generatedUid;
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values.Add("DeviceUID", deviceUid);
                    return deviceUid;
                }
            }
        }

        /// <summary>
        /// Gets the application bundle.
        /// </summary>
        /// <returns>The application bundle name.</returns>
        internal static String GetAppBundle()
        {
            try
            {
                return Package.Current.Id.Name;
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting app bundle", ex.ToString());
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <returns>The name of the application</returns>
        internal static async Task<String> GetAppName()
        {
            try
            {
                StorageFile file = await Package.Current.InstalledLocation.GetFileAsync("AppxManifest.xml");
                string manifestXml = await FileIO.ReadTextAsync(file);
                XDocument doc = XDocument.Parse(manifestXml);
                XNamespace packageNamespace = "http://schemas.microsoft.com/appx/2010/manifest";
                var displayName = (from name in doc.Descendants(packageNamespace + "DisplayName")
                                   select name.Value).First();
                return displayName;
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting app name", ex.ToString());
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the application version.
        /// </summary>
        /// <returns>The application version</returns>
        internal static String GetAppVersion()
        {
            string appVersion = string.Empty;
            try
            {
                appVersion = string.Format("{0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error getting app version", ex.ToString());
            }

            return appVersion;
        }
    }
}
