using System;
using System.Collections.Generic;
using System.Text;

namespace LiquidTestProject
{
    public class MainApp
    {
        public static string defaultTitle = "Welcome to our app";
        public static string defaultBgColor = "#FF0000";
        public static string defaultPromoDay = "2014-05-11T15:17:03.103+0100";
        public static int defaultLoginVersion = 3;
        public static float defaultDiscount = 0.15f;
        public static bool defaultShowAds = true;

        public static Dictionary<string, Dictionary<string, object>> userProfiles = new Dictionary<string, Dictionary<string, object>>();
        public static string selectedUserProfile;
    }
}
