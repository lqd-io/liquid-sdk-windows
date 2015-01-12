using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using LiquidWindowsSDK;
using LiquidWindowsSDK.Model;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json.Linq;

namespace LiquidSDKWindowsPhoneTest
{
    [TestClass]
    public class LiquidSDKWindowsTargetsInvalidationTest
    {

        public async Task InitializeDynamicVariablesTests()
        {
            Liquid._instance = new Liquid
            {
                _device = new LQDevice(Liquid.LIQUID_VERSION),
                _currentUser = new LQUser(LQModel.NewIdentifier())
            };
            string fileContent;
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///JsonStub/liquid_package_targets.json"));
            using (var sRead = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                fileContent = await sRead.ReadToEndAsync();
            }
            JObject jsonObject = JObject.Parse(fileContent);
            Liquid.Instance._loadedLiquidPackage = new LQLiquidPackage(jsonObject);
            Liquid.Instance._appliedValues = LQValue.ConvertToDictionary(Liquid.Instance._loadedLiquidPackage.Values);
        }

        public async Task<JObject> TrackEvent()
        {
            await InitializeDynamicVariablesTests();
            await Liquid.Instance.GetIntVariable("freeCoins", 1);
            Liquid.Instance._httpQueuer = new LQQueuer("liquid_tests");
            Liquid.Instance.Track("Click Button");
            await Task.Delay(5000);
            var lastRequest = Liquid.Instance._httpQueuer.Queue.Last();
            return JObject.Parse(lastRequest.Json);
        }

        [TestMethod]
        public async Task InvalidateFreeCoinsTestMethod()
        {
            await InitializeDynamicVariablesTests();
            var freeCoins = await Liquid.Instance.GetIntVariable("freeCoins", 1);
            Assert.AreEqual(1, freeCoins);
        }

        [TestMethod]
        public async Task InvalidateDiscountTestMethod()
        {
            const float fallbackValue = 0.1f;

            await InitializeDynamicVariablesTests();
            await Liquid.Instance.GetIntVariable("freeCoins", 1);
            var discount = await Liquid.Instance.GetFloatVariable("discount", fallbackValue);
            Assert.AreEqual(fallbackValue, discount);
        }

        [TestMethod]
        public async Task NotInvalidateTitleTestMethod()
        {
            const string fallbackValue = "Fallback value";
            const string serverFallbackValue = "Default value of this variable";

            await InitializeDynamicVariablesTests();
            await Liquid.Instance.GetIntVariable("freeCoins", 1);
            var title = await Liquid.Instance.GetStringVariable("title", fallbackValue);
            Assert.AreEqual(serverFallbackValue, title);
        }

        [TestMethod]
        public async Task NotInvalidateShowDateTestMethod()
        {
            var fallbackValue = new DateTime(1970, 1, 1);

            await InitializeDynamicVariablesTests();
            await Liquid.Instance.GetIntVariable("freeCoins", 1);
            var showDate = await Liquid.Instance.GetDateVariable("showDate", fallbackValue);
            Assert.AreNotEqual(fallbackValue, showDate);
        }

        [TestMethod]
        public async Task NotInvalidateBackgroundColorTestMethod()
        {
            var fallbackValue = Colors.Blue;
            var serverValue = Colors.Red;

            await InitializeDynamicVariablesTests();
            await Liquid.Instance.GetIntVariable("freeCoins", 1);
            var backgroundColor = await Liquid.Instance.GetColorVariable("backgroundColor", fallbackValue);
            Assert.AreEqual(serverValue, backgroundColor);
        }

        [TestMethod]
        public async Task NotInvalidateShowAdsTestMethod()
        {
            const bool fallbackValue = false;
            const bool serverValue = true;

            await InitializeDynamicVariablesTests();
            await Liquid.Instance.GetIntVariable("freeCoins", 1);
            var showAds = await Liquid.Instance.GetBooleanVariable("showAds", fallbackValue);
            Assert.AreEqual(serverValue, showAds);
        }

        [TestMethod]
        public async Task TrackTestMethodAndCheckInvalidatedTarget()
        {
            var jsonDictionary = await TrackEvent();
            int numberOfValuesWithTarget = jsonDictionary["values"].Count(value => value["target_id"].ToString().Equals("d8b035d088469702d6c53800"));
            Assert.AreEqual(0, numberOfValuesWithTarget);
        }

        [TestMethod]
        public async Task TrackTestMethodAndCheckValidTarget()
        {
            var jsonDictionary = await TrackEvent();
            int numberOfValuesWithTarget = jsonDictionary["values"].Count(value => value["target_id"].ToString().Equals("9702d388538a062ca6900000"));
            Assert.AreEqual(1, numberOfValuesWithTarget);
        }

        [TestMethod]
        public async Task TrackTestMethodAndCheckNoTarget()
        {
            var jsonDictionary = await TrackEvent();
            int numberOfValuesWithTarget = jsonDictionary["values"].Count(value => !value["target_id"].ToString().Equals(string.Empty));
            Assert.AreEqual(1, numberOfValuesWithTarget);
        }

        [TestMethod]
        public async Task TrackTestMethodAndCheckNumberOfValues()
        {
            var jsonDictionary = await TrackEvent();
            Assert.AreEqual(4, jsonDictionary["values"].Count());
        }

        [TestMethod]
        public async Task TrackTestMethodAndCheckForTitleValue()
        {
            var jsonDictionary = await TrackEvent();
            bool valueFound = jsonDictionary["values"].Any(value => value["id"].ToString().Equals("5371978369702d37ca180000"));
            Assert.IsTrue(valueFound);
        }

        [TestMethod]
        public async Task TrackTestMethodAndCheckForFreeCoinsValue()
        {
            var jsonDictionary = await TrackEvent();
            bool valueFound = jsonDictionary["values"].Any(value => value["id"].ToString().Equals("538382ca69702d08900c0600"));
            Assert.IsFalse(valueFound);
        }

        [TestMethod]
        public async Task TrackTestMethodAndCheckForDiscountValue()
        {
            var jsonDictionary = await TrackEvent();
            bool valueFound = jsonDictionary["values"].Any(value => value["id"].ToString().Equals("538382ca69702d08900a0600"));
            Assert.IsFalse(valueFound);
        }
    }
}