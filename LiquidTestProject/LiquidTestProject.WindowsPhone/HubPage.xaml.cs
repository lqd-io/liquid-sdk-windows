using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using LiquidTestProject.Common;
using System;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Universal Hub Application project template is documented at Http://go.microsoft.com/fwlink/?LinkID=391955
using LiquidWindowsSDK;
using LiquidWindowsSDK.Model;

namespace LiquidTestProject
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage
    {
        private bool isProgramChanging = true;
        internal readonly NavigationHelper navigationHelper;
        internal readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        internal readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public HubPage()
        {
            InitializeComponent();


            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            NavigationCacheMode = NavigationCacheMode.Required;

            navigationHelper = new NavigationHelper(this);
            navigationHelper.LoadState += NavigationHelper_LoadState;
            navigationHelper.SaveState += NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        internal void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {

        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        internal void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        /// <summary>
        /// Shows the details of a clicked group in the <see>
        ///         <cref>SectionPage</cref>
        ///     </see>
        ///     .
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Details about the click event.</param>
        internal void GroupSection_ItemClick(object sender, ItemClickEventArgs e)
        {
        }

        /// <summary>
        /// Shows the details of an item clicked on in the <see>
        ///         <cref>ItemPage</cref>
        ///     </see>
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Defaults about the click event.</param>
        internal void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see>
        ///     <cref>NavigationHelper.LoadState</cref>
        /// </see>
        ///     and <see>
        ///         <cref>NavigationHelper.SaveState</cref>
        ///     </see>
        ///     .
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);


            isProgramChanging = true;
            string currentUserIdentifier = Liquid.Instance.GetUserIdentifier();
            MainApp.selectedUserProfile = currentUserIdentifier;
            userIdentifierTbk.Text = currentUserIdentifier;

            tgb100.IsChecked = false;
            tgb101.IsChecked = false;
            tgb102.IsChecked = false;
            tgb103.IsChecked = false;
            tgbAnonymous.IsChecked = false;

            if (currentUserIdentifier.Equals("100"))
            {
                tgb100.IsChecked = true;
            }
            else if (currentUserIdentifier.Equals("101"))
            {
                tgb101.IsChecked = true;
            }
            else if (currentUserIdentifier.Equals("102"))
            {
                tgb102.IsChecked = true;
            }
            else if (currentUserIdentifier.Equals("103"))
            {
                tgb103.IsChecked = true;
            }
            else
            {
                tgbAnonymous.IsChecked = true;
            }

            ReloadData();

            isProgramChanging = false;

            Liquid.ValuesLoaded += LiquidOnValuesLoaded;
            Liquid.ValuesReceived += LiquidOnValuesReceived;
            LQPushHandler.PushNotificationReceived += LqPushHandlerOnPushNotificationReceived;

            await RefreshInformation();

        }

        private async Task RefreshInformation()
        {
            var title = await Liquid.Instance.GetStringVariable("title", MainApp.defaultTitle);
            var promoDay = (DateTime)await Liquid.Instance.GetDateVariable("promoDay", ISO8601Utils.Parse(MainApp.defaultPromoDay));
            Color bgColor = await Liquid.Instance.GetColorVariable("bgColor", LiquidTools.HexToColor(MainApp.defaultBgColor));
            int loginVersion = await Liquid.Instance.GetIntVariable("login", MainApp.defaultLoginVersion);
            float discount = await Liquid.Instance.GetFloatVariable("discount", MainApp.defaultDiscount);
            bool showAds = await Liquid.Instance.GetBooleanVariable("showAds", MainApp.defaultShowAds);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bgColorRectangle.Fill = new SolidColorBrush(bgColor);
                bgColorTbk.Text = LiquidTools.ColorToHex(bgColor);
                showAdsTbk.Text = showAds.ToString();
                titleTbk.Text = title;
            });

            Debug.WriteLine("title: {0}", title);
            Debug.WriteLine("promoDay: {0}", promoDay);
            Debug.WriteLine("bgColor: {0}", bgColor);
            Debug.WriteLine("loginVersion: {0}", loginVersion);
            Debug.WriteLine("discount: {0}", discount);
            Debug.WriteLine("showAds: {0}", showAds);
        }

        private async void LqPushHandlerOnPushNotificationReceived(object sender, PushNotificationReceivedEventArgs pushNotificationReceivedEventArgs)
        {
            Debug.WriteLine("Push Notification received");

            await new MessageDialog("Do whatever you want with this notification.", "Push Notification received").ShowAsync();
        }

        private void LiquidOnValuesReceived(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("Received new values from Liquid Server. They were stored in cache, waiting to be loaded.");
        }

        internal static SemaphoreSlim _loadLock = new SemaphoreSlim(1);
        private async void LiquidOnValuesLoaded(object sender, EventArgs eventArgs)
        {
            //await _loadLock.WaitAsync();
            await RefreshInformation();
            Debug.WriteLine("Cached values were loaded into memory.");
            //_loadLock.Release();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void ResetSdkButtonOnClick(object sender, RoutedEventArgs e)
        {
            await Liquid.Instance.SoftReset();
        }

        private async void FlushHTTPHeadersOnClick(object sender, RoutedEventArgs e)
        {
            await Liquid.Instance.Flush();
        }

        private void PrintIdentifiersOnClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("User unique_id: {0}", Liquid.Instance.GetUserIdentifier());
            Debug.WriteLine("Device unique_id: {0}", LQDevice.GetDeviceID());
            Debug.WriteLine("Session unique_id: {0}", Liquid.Instance.GetSessionIdentifier());
        }

        private void TrackBuyProductOnClick(object sender, RoutedEventArgs e)
        {
            Liquid.Instance.Track("Buy Product", new Dictionary<string, object>
            {
                {"productId", 40},
                {"price", 30.5},
                {"withDiscount", true},
            });

            Debug.WriteLine("Track 'Buy Product' event");
        }

        private void TrackPlayMusicOnClick(object sender, RoutedEventArgs e)
        {
            Liquid.Instance.Track("Play Music", new Dictionary<string, object>
            {
                {"artist", "Bee Gees"},
                {"track", "Stayin' Alive"},
                {"album", "Saturday Night Fever"},
                {"releaseYear", 1997},
            });

            Debug.WriteLine("Track 'Play Music' event");
        }

        private void TrackCustomEventOnClick(object sender, RoutedEventArgs e)
        {
            Liquid.Instance.Track(customEventTbx.Text);

            Debug.WriteLine("Track '{0}' event", customEventTbx.Text);
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (autoLoadTgs.IsOn)
            {
                Liquid.Instance.AutoLoadValues = true;
                loadValuesBtn.IsEnabled = false;
                Debug.WriteLine("Auto load values (when new values are received from Liquid server) is now ON");
            }
            else
            {
                Liquid.Instance.AutoLoadValues = false;
                loadValuesBtn.IsEnabled = true;
                Debug.WriteLine("Auto load values (when new values are received from Liquid server) is now OFF");
            }
        }

        private async void RequestValuesOnClick(object sender, RoutedEventArgs e)
        {
            await Liquid.Instance.RequestValues();
        }

        private async void LoadValuesOnClick(object sender, RoutedEventArgs e)
        {
            await Liquid.Instance.LoadValues();
        }

        private async void tgbAnonymous_Checked(object sender, RoutedEventArgs e)
        {
            if (isProgramChanging)
            {
                return;
            }
            await Liquid.Instance.ResetUser();
            MainApp.selectedUserProfile = Liquid.Instance.GetUserIdentifier();
            userIdentifierTbk.Text = Liquid.Instance.GetUserIdentifier();

            userAge.Text = string.Empty;
            userName.Text = string.Empty;
            userGender.Text = string.Empty;

            isProgramChanging = true;
            tgb100.IsChecked = false;
            tgb101.IsChecked = false;
            tgb102.IsChecked = false;
            tgb103.IsChecked = false;
            isProgramChanging = false;
        }

        private void TgbAnonymous_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (isProgramChanging)
            {
                return;
            }
            isProgramChanging = true;
            tgbAnonymous.IsChecked = true;
            isProgramChanging = false;
        }

        private async void Tgb100_OnChecked(object sender, RoutedEventArgs e)
        {
            if (isProgramChanging)
            {
                return;
            }
            await SetCurrentUserWithIdentifier("100");
            isProgramChanging = true;
            tgbAnonymous.IsChecked = false;
            tgb101.IsChecked = false;
            tgb102.IsChecked = false;
            tgb103.IsChecked = false;
            isProgramChanging = false;
        }

        private void Tgb100_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (isProgramChanging)
            {
                return;
            }
            isProgramChanging = true;
            tgb100.IsChecked = true;
            isProgramChanging = false;
        }

        private async void Tgb101_OnChecked(object sender, RoutedEventArgs e)
        {
            if (isProgramChanging)
            {
                return;
            }
            await SetCurrentUserWithIdentifier("101");
            isProgramChanging = true;
            tgbAnonymous.IsChecked = false;
            tgb100.IsChecked = false;
            tgb102.IsChecked = false;
            tgb103.IsChecked = false;
            isProgramChanging = false;
        }

        private void Tgb101_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (isProgramChanging)
            {
                return;
            }
            isProgramChanging = true;
            tgb101.IsChecked = true;
            isProgramChanging = false;
        }

        private async void Tgb102_OnChecked(object sender, RoutedEventArgs e)
        {
            if (isProgramChanging)
            {
                return;
            }
            await SetCurrentUserWithIdentifier("102");
            isProgramChanging = true;
            tgbAnonymous.IsChecked = false;
            tgb100.IsChecked = false;
            tgb101.IsChecked = false;
            tgb103.IsChecked = false;
            isProgramChanging = false;
        }

        private void Tgb102_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (isProgramChanging)
            {
                return;
            }
            isProgramChanging = true;
            tgb102.IsChecked = true;
            isProgramChanging = false;
        }

        private async void Tgb103_OnChecked(object sender, RoutedEventArgs e)
        {
            if (isProgramChanging)
            {
                return;
            }
            await SetCurrentUserWithIdentifier("103");
            isProgramChanging = true;
            tgbAnonymous.IsChecked = false;
            tgb100.IsChecked = false;
            tgb101.IsChecked = false;
            tgb102.IsChecked = false;
            isProgramChanging = false;
        }

        private void Tgb103_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (isProgramChanging)
            {
                return;
            }
            isProgramChanging = true;
            tgb103.IsChecked = true;
            isProgramChanging = false;
        }



        private async Task SetCurrentUserWithIdentifier(string identifier)
        {
            var userAttributes = MainApp.userProfiles[identifier];
            await Liquid.Instance.IdentifyUser(identifier, userAttributes);
            MainApp.selectedUserProfile = Liquid.Instance.GetUserIdentifier();
            userIdentifierTbk.Text = identifier;

            ReloadData();
        }

        private void ReloadData()
        {
            if (MainApp.selectedUserProfile != "100" &&
                MainApp.selectedUserProfile != "101" &&
                MainApp.selectedUserProfile != "102" &&
                MainApp.selectedUserProfile != "103")
            {
                userAge.Text = string.Empty;
                userName.Text = string.Empty;
                userGender.Text = string.Empty;
            }
            else
            {
                userAge.Text = MainApp.userProfiles[MainApp.selectedUserProfile]["age"].ToString();
                userName.Text = MainApp.userProfiles[MainApp.selectedUserProfile]["name"].ToString();
                userGender.Text = MainApp.userProfiles[MainApp.selectedUserProfile]["gender"].ToString();
            }

        }
    }
}