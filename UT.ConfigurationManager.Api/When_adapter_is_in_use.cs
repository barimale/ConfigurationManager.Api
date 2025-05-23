using ConfigurationManager.Api;
using ConfigurationManager.Api.Helper.Adapters;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UT.ConfigurationManager.Api
{
    public class When_adapter_is_in_use
    {
        private IManager service;
        private IManager addedFolder;
        private string appFolder = Guid.NewGuid().ToString().ToUpper();
        private KeyValuePair<string, string> firstPair = new KeyValuePair<string, string>("keyOne", "valueOne");
        private KeyValuePair<string, string> secondPair = new KeyValuePair<string, string>("keyTwo", "valueTwo");

        [OneTimeTearDown]
        public void CleanUp()
        {
            service.RemoveFolderAsync(addedFolder);
        }

        [OneTimeSetUp]
        public async Task Setup()
        {
            service = new Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName)
                .AsManager();

            addedFolder = await service.AddFolderAsync(appFolder).ConfigureAwait(false);
            var appSettingsFolder = await addedFolder.AddFolderAsync(EagerAdapter.AppSettingsName).ConfigureAwait(false);
            await appSettingsFolder.AddAsync(firstPair.Key, firstPair.Value).ConfigureAwait(false);
            await appSettingsFolder.AddAsync(secondPair.Key, secondPair.Value).ConfigureAwait(false);

            var connectionStringFolder = await addedFolder.AddFolderAsync(EagerAdapter.ConnectionStringsName).ConfigureAwait(false);
            await connectionStringFolder.AddAsync(firstPair.Key, firstPair.Value).ConfigureAwait(false);
            await connectionStringFolder.AddAsync(secondPair.Key, secondPair.Value).ConfigureAwait(false);
        }

        [Test]
        public async Task I_d_like_to_get_specific_appSetting_using_lazy_adapter()
        {
            //given 
            IReadOnly readOnlyService = new Manager(
                InputData.HostName, 
                InputData.Port, 
                InputData.ServiceHostName,
                appFolder)
                .AsReadOnly();

            var lazyAdapter = new LazyAdapter(readOnlyService);

            //when
            var appSettingsValueOfTheKey = lazyAdapter.AppSettings(firstPair.Key);
            var appSettingsValueOfTheSecondKey = lazyAdapter.AppSettings(secondPair.Key);

            //then
            ClassicAssert.AreEqual(appSettingsValueOfTheKey, firstPair.Value);
            ClassicAssert.AreEqual(appSettingsValueOfTheSecondKey, secondPair.Value);
        }

        [Test]
        public async Task I_d_like_to_get_specific_connectionString_using_lazy_adapter()
        {
            //given 
            IReadOnly readOnlyService = new Manager(
               InputData.HostName,
               InputData.Port,
               InputData.ServiceHostName,
               appFolder)
                .AsReadOnly();

            var lazyAdapter = new LazyAdapter(readOnlyService);

            //when
            var appSettingsValueOfTheKey = lazyAdapter.ConnectionStrings(firstPair.Key);
            var appSettingsValueOfTheSecondKey = lazyAdapter.ConnectionStrings(secondPair.Key);

            //then
            ClassicAssert.AreEqual(appSettingsValueOfTheKey, firstPair.Value);
            ClassicAssert.AreEqual(appSettingsValueOfTheSecondKey, secondPair.Value);
        }

        [Test]
        public async Task I_d_like_to_get_specific_appSetting_using_eager_adapter()
        {
            //given 
            IReadOnly readOnlyService = new Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName,
                appFolder)
                .AsReadOnly();

            var eagerAdapter = new EagerAdapter(readOnlyService);

            //when
            var appSettingsValueOfTheKey = eagerAdapter.AppSettings(firstPair.Key);
            var appSettingsValueOfTheSecondKey = eagerAdapter.AppSettings(secondPair.Key);

            //then
            ClassicAssert.AreEqual(appSettingsValueOfTheKey, firstPair.Value);
            ClassicAssert.AreEqual(appSettingsValueOfTheSecondKey, secondPair.Value);
        }

        [Test]
        public async Task I_d_like_to_get_specific_connectionString_using_eager_adapter()
        {
            //given 
            IReadOnly readOnlyService = new Manager(
               InputData.HostName,
               InputData.Port,
               InputData.ServiceHostName,
               appFolder)
                .AsReadOnly();

            var eagerAdapter = new EagerAdapter(readOnlyService);

            //when
            var appSettingsValueOfTheKey = eagerAdapter.ConnectionStrings(firstPair.Key);
            var appSettingsValueOfTheSecondKey = eagerAdapter.ConnectionStrings(secondPair.Key);

            //then
            ClassicAssert.AreEqual(appSettingsValueOfTheKey, firstPair.Value);
            ClassicAssert.AreEqual(appSettingsValueOfTheSecondKey, secondPair.Value);
        }
    }
}