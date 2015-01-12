using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Windows.Storage;

namespace LiquidWindowsSDK
{
    public class LQPushHandler
    {
        /// <summary>
        /// Occurs when a Push Notification is received.
        /// </summary>
        public static event EventHandler<PushNotificationReceivedEventArgs> PushNotificationReceived;
        protected static void OnPushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            EventHandler<PushNotificationReceivedEventArgs> handler = PushNotificationReceived;
            if (handler != null)
            {
                handler(sender, args);
            }
        }

        internal static async Task CreateNotificationChannel()
        {
            PushNotificationChannel channel = null;

            try
            {
                channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            }

            catch (Exception ex)
            {
                Debug.WriteLine("Could not create a channel.");
                Debug.WriteLine(ex);
            }

            if (channel != null)
            {
                try
                {
                    channel.PushNotificationReceived += ChannelOnPushNotificationReceived;
                    Liquid.Instance.SetWNSDeviceUri(channel.Uri);
                    ApplicationData.Current.LocalSettings.Values["channelUri"] = channel.Uri;
                    LQLog.InfoVerbose(channel.Uri);
                }
                catch (Exception ex)
                {
                    LiquidTools.LogUnexpectedException("Unexpected error setting Push Notifications", ex.ToString());
                }
            }
        }

        internal static void ChannelOnPushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            LQLog.InfoVerbose("Push Notification Received: " + args.NotificationType);
            OnPushNotificationReceived(sender, args);
        }
    }
}
