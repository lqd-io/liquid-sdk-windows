using System;
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
    public class LiquidSDKWindowsValuesInvalidationTest
    {

        public async Task InitializeDynamicVariablesWithCorrectDataTypesTests()
        {
            Liquid._instance = new Liquid { _device = new LQDevice(Liquid.LIQUID_VERSION) };
            string fileContent;
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///JsonStub/liquid_package_correct_data_types.json"));
            using (var sRead = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                fileContent = await sRead.ReadToEndAsync();
            }
            JObject jsonObject = JObject.Parse(fileContent);
            Liquid.Instance._loadedLiquidPackage = new LQLiquidPackage(jsonObject);
            Liquid.Instance._appliedValues = LQValue.ConvertToDictionary(Liquid.Instance._loadedLiquidPackage.Values);
            await Liquid.Instance.IdentifyUser("333", null, false);
            await Task.Delay(100);
            await Liquid.Instance._loadedLiquidPackage.SaveToDisk();
            await Liquid.Instance.LoadValues();
            await Task.Delay(100);
        }

        public async Task InitializeDynamicVariablesWithIncorrectDataTypesTests()
        {
            Liquid._instance = new Liquid { _device = new LQDevice(Liquid.LIQUID_VERSION) };
            string fileContent;
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///JsonStub/liquid_package_incorrect_data_types.json"));
            using (var sRead = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                fileContent = await sRead.ReadToEndAsync();
            }
            JObject jsonObject = JObject.Parse(fileContent);
            Liquid.Instance._loadedLiquidPackage = new LQLiquidPackage(jsonObject);
            Liquid.Instance._appliedValues = LQValue.ConvertToDictionary(Liquid.Instance._loadedLiquidPackage.Values);
            await Liquid.Instance.IdentifyUser("333", null, false);
            await Task.Delay(100);
            await Liquid.Instance._loadedLiquidPackage.SaveToDisk();
            await Liquid.Instance.LoadValues();
            await Task.Delay(100);
        }

        [TestMethod]
        public async Task CheckVariablesCount()
        {
            await InitializeDynamicVariablesWithCorrectDataTypesTests();
            var valuesCount = Liquid.Instance._loadedLiquidPackage.Values.Count();
            Assert.AreEqual(6, valuesCount);
        }

        [TestMethod]
        public async Task NotInvalidateTitleValueTestMethod()
        {
            await InitializeDynamicVariablesWithCorrectDataTypesTests();

            const string fallbackValue = @"Fallback value";
            const string serverValue = @"Default value of this variable";
            var title = await Liquid.Instance.GetStringVariable("title", fallbackValue);
            Assert.AreEqual(serverValue, title);
        }

        [TestMethod]
        public async Task NotInvalidateShowDateValueTestMethod()
        {
            await InitializeDynamicVariablesWithCorrectDataTypesTests();

            var fallbackValue = new DateTime(1970, 1, 1);
            var title = await Liquid.Instance.GetDateVariable("showDate", fallbackValue);
            Assert.AreNotEqual(fallbackValue, title);
        }

        [TestMethod]
        public async Task NotInvalidateBackgroundColorValueTestMethod()
        {
            var fallbackValue = Colors.Blue;
            var serverValue = Colors.Red;

            await InitializeDynamicVariablesWithCorrectDataTypesTests();
            var backgroundColor = await Liquid.Instance.GetColorVariable("backgroundColor", fallbackValue);
            Assert.AreEqual(serverValue, backgroundColor);
        }

        [TestMethod]
        public async Task NotInvalidateShowAdsValueTestMethod()
        {
            const bool fallbackValue = false;
            const bool serverValue = true;

            await InitializeDynamicVariablesWithCorrectDataTypesTests();
            var showAds = await Liquid.Instance.GetBooleanVariable("showAds", fallbackValue);
            Assert.AreEqual(serverValue, showAds);
        }

        [TestMethod]
        public async Task NotInvalidateDiscountValueTestMethod()
        {
            const float fallbackValue = 0.1f;
            const float serverValue = 0.25f;

            await InitializeDynamicVariablesWithCorrectDataTypesTests();
            var discount = await Liquid.Instance.GetFloatVariable("discount", fallbackValue);
            Assert.AreEqual(serverValue, discount);
        }

        [TestMethod]
        public async Task NotInvalidateFreeCoinsValueTestMethod()
        {
            const int fallbackValue = 1;
            const int serverValue = 7;

            await InitializeDynamicVariablesWithCorrectDataTypesTests();
            var freeCoins = await Liquid.Instance.GetIntVariable("freeCoins", fallbackValue);
            Assert.AreEqual(serverValue, freeCoins);
        }

        [TestMethod]
        public async Task InvalidateUnknownVariableValueTestMethod()
        {
            await InitializeDynamicVariablesWithIncorrectDataTypesTests();

            const string fallbackValue = @"Fallback value";
            var title = await Liquid.Instance.GetStringVariable("unknownVariable", fallbackValue);
            Assert.AreEqual(fallbackValue, title);
        }

        [TestMethod]
        public async Task InvalidateTitleValueTestMethod()
        {
            await InitializeDynamicVariablesWithIncorrectDataTypesTests();

            const string fallbackValue = @"Fallback value";
            var title = await Liquid.Instance.GetStringVariable("title", fallbackValue);
            Assert.AreEqual(fallbackValue, title);
        }

        [TestMethod]
        public async Task InvalidateShowDateValueTestMethod()
        {
            await InitializeDynamicVariablesWithIncorrectDataTypesTests();

            var fallbackValue = new DateTime(1970, 1, 1);
            var title = await Liquid.Instance.GetDateVariable("showDate", fallbackValue);
            Assert.AreEqual(fallbackValue, title);
        }

        [TestMethod]
        public async Task InvalidateBackgroundColorValueTestMethod()
        {
            var fallbackValue = Colors.Blue;

            await InitializeDynamicVariablesWithIncorrectDataTypesTests();
            var backgroundColor = await Liquid.Instance.GetColorVariable("backgroundColor", fallbackValue);
            Assert.AreEqual(fallbackValue, backgroundColor);
        }

        [TestMethod]
        public async Task InvalidateShowAdsValueTestMethod()
        {
            const bool fallbackValue = false;

            await InitializeDynamicVariablesWithIncorrectDataTypesTests();
            var showAds = await Liquid.Instance.GetBooleanVariable("showAds", fallbackValue);
            Assert.AreEqual(fallbackValue, showAds);
        }

        [TestMethod]
        public async Task InvalidateDiscountValueTestMethod()
        {
            const float fallbackValue = 0.1f;

            await InitializeDynamicVariablesWithIncorrectDataTypesTests();
            var discount = await Liquid.Instance.GetFloatVariable("discount", fallbackValue);
            Assert.AreEqual(fallbackValue, discount);
        }

        [TestMethod]
        public async Task InvalidateFreeCoinsValueTestMethod()
        {
            const int fallbackValue = 1;

            await InitializeDynamicVariablesWithIncorrectDataTypesTests();
            var freeCoins = await Liquid.Instance.GetIntVariable("freeCoins", fallbackValue);
            Assert.AreEqual(fallbackValue, freeCoins);
        }
    }
}