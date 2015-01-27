﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using LiquidTestProject.Common;

// The Universal Hub Application project template is documented at Http://go.microsoft.com/fwlink/?LinkID=391955
using LiquidWindowsSDK;

namespace LiquidTestProject
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App
    {
#if WINDOWS_PHONE_APP
        internal TransitionCollection transitions;
#endif
        internal Geolocator _geolocator;

        /// <summary>
        /// Initializes the singleton instance of the <see cref="App"/> class. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                //DebugSettings.EnableFrameRateCounter = true;
            }
#endif

#if DEBUG
            await Liquid.Initialize("YOUR_DEVELOPMENT_APP_TOKEN", this, e, true);
#else
            await Liquid.Initialize("YOUR_PRODUCTION_APP_TOKEN", this, e);
#endif


            await Liquid.Instance.SetupPushNotifications();

            if (!MainApp.userProfiles.ContainsKey("100"))
            {
                MainApp.userProfiles.Add("100", new Dictionary<string, object>()
                {
                    {"name", "Anna Martinez"},
                    {"age", 25},
                    {"gender", "female"},
                });

                MainApp.userProfiles.Add("101", new Dictionary<string, object>()
                {
                    {"name", "John Clark"},
                    {"age", 37},
                    {"gender", "male"},
                });
                MainApp.userProfiles.Add("102", new Dictionary<string, object>()
                {
                    {"name", "Barry Hill"},
                    {"age", 16},
                    {"gender", "male"},
                });
                MainApp.userProfiles.Add("103", new Dictionary<string, object>()
                {
                    {"name", "Guilherme Alves"},
                    {"age", 1},
                    {"gender", "male"},
                });
            }


            var rootFrame = Window.Current.Content as Frame;


            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(HubPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            _geolocator = new Geolocator { DesiredAccuracy = PositionAccuracy.High, MovementThreshold = 1 };
            _geolocator.StatusChanged += GeolocatorOnStatusChanged;
            _geolocator.PositionChanged += GeolocatorOnPositionChanged;

            #region Test code for singleThread

            #endregion

            // Ensure the current window is active
            Window.Current.Activate();
        }

        internal void GeolocatorOnStatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            try
            {
                Debug.WriteLine("New Status!\n" + args.Status);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        internal async void GeolocatorOnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            try
            {
                await Liquid.Instance.SetCurrentLocation(args.Position.Coordinate);
                Debug.WriteLine("New Position!\n" + args.Position.Coordinate.Point.Position.Latitude + "\n" + args.Position.Coordinate.Point.Position.Longitude);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        internal void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = transitions ?? new TransitionCollection { new NavigationThemeTransition() };
            rootFrame.Navigated -= RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        internal async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}