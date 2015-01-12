using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using LiquidWindowsSDK;
using LiquidWindowsSDK.Model;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json.Linq;

namespace LiquidSDKWindowsPhoneTest
{
    [TestClass]
    public class LiquidSDKWindowsIntegrationTest
    {

        public void InitializeIntegrationTests()
        {
            Liquid._instance = new Liquid
            {
                _device = new LQDevice(Liquid.LIQUID_VERSION),
                _currentUser = new LQUser(LQModel.NewIdentifier())
            };
        }

        public async Task InitializeDynamicVariablesAndIdentifyUserTests()
        {
            Liquid._instance = new Liquid
            {
                _device = new LQDevice(Liquid.LIQUID_VERSION),
                _currentUser = new LQUser(LQModel.NewIdentifier())
            };
            string fileContent;
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///JsonStub/liquid_package1.json"));
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

        public async Task GoToSleepAndAwake()
        {
            string fileContent;
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///JsonStub/liquid_package1.json"));
            using (var sRead = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                fileContent = await sRead.ReadToEndAsync();
            }
            JObject jsonObject = JObject.Parse(fileContent);
            Liquid.Instance._loadedLiquidPackage = new LQLiquidPackage(jsonObject);
            Liquid.Instance._appliedValues = LQValue.ConvertToDictionary(Liquid.Instance._loadedLiquidPackage.Values);
            Liquid.Instance._httpQueuer = new LQQueuer("liquid_tests");
            await Task.Delay(100);
            await Liquid.Instance._loadedLiquidPackage.SaveToDisk();
            await Liquid.Instance.LoadValues();
            await Task.Delay(100);
        }

        [TestMethod]
        public async Task InvalidateWelcomeTestTestMethod()
        {
            InitializeIntegrationTests();

            const string fallbackValue = @"Fallback test";
            var title = await Liquid.Instance.GetStringVariable("welcomeText", fallbackValue);
            Assert.AreEqual(fallbackValue, title);
        }

        [TestMethod]
        public async Task ValidateWelcomeMessageTestMethod()
        {
            InitializeIntegrationTests();
            await GoToSleepAndAwake();

            const string fallbackValue = @"A Fallback Value";
            const string serverValue = @"Be very welcome";
            var title = await Liquid.Instance.GetStringVariable("welcomeMessage", fallbackValue);
            Assert.AreEqual(serverValue, title);
        }

        [TestMethod]
        public async Task InvalidateDiscountTestMethod()
        {
            InitializeIntegrationTests();
            await GoToSleepAndAwake();

            const string fallbackValue = @"Welcome to Liquid";
            var title = await Liquid.Instance.GetStringVariable("discount", fallbackValue);
            Assert.AreEqual(fallbackValue, title);
        }

        [TestMethod]
        public async Task InvalidateUnknownVariableTestMethod()
        {
            InitializeIntegrationTests();
            await GoToSleepAndAwake();

            const string fallbackValue = @"A Fallback Value";
            var title = await Liquid.Instance.GetStringVariable("anUnkownVariable", fallbackValue);
            Assert.AreEqual(fallbackValue, title);
        }

        [TestMethod]
        public async Task MultipleOperationsTest()
        {
            await InitializeDynamicVariablesAndIdentifyUserTests();

            int totalOfBlocks = 0;

            for (int i = 0; i < 20; i++)
            {
                await SingleThreadExecutor.RunAsync(async () =>
                {
                    await Liquid.Instance.ApplicationSuspendedCallback();
                    await Task.Delay(250);
                    Liquid.Instance.ApplicationLauchedOrResumedCallback(null);
                    totalOfBlocks++;
                });
            }

            for (int i = 0; i < 20; i++)
            {
                await SingleThreadExecutor.RunAsync(async () =>
                {
                    await Task.Delay(100);
                    totalOfBlocks++;
                });
            }

            for (int i = 0; i < 20; i++)
            {
                await SingleThreadExecutor.RunAsync(async () =>
                {
                    await Liquid.Instance.RequestValues(false);
                    totalOfBlocks++;
                });
            }

            for (int i = 0; i < 20; i++)
            {
                await SingleThreadExecutor.RunAsync(async () =>
                {
                    await Liquid.Instance.LoadValues();
                    totalOfBlocks++;
                });
            }

            for (int i = 0; i < 20; i++)
            {
                await SingleThreadExecutor.RunAsync(async () =>
                {
                    const string fallbackValue = @"A Fallback Value";
                    await Liquid.Instance.GetStringVariable("welcomeText", fallbackValue);
                    totalOfBlocks++;
                });
            }

            for (int i = 0; i < 20; i++)
            {
                await SingleThreadExecutor.RunAsync(async () =>
                {
                    const string fallbackValue = @"A Fallback Value";
                    await Liquid.Instance.GetStringVariable("unknownVariable", fallbackValue);
                    totalOfBlocks++;
                });
            }
            await Task.Delay(20000);
            Assert.AreEqual(120, totalOfBlocks);
        }
    }
}