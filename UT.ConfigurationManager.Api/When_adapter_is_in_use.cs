using ConfigurationManager.Api;
using ConfigurationManager.Api.Helper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UT.ConfigurationManager.Api
{
    public class When_adapter_is_in_use
    {
        private string appFolder = Guid.NewGuid().ToString();
        private KeyValuePair<string, string> firstPair = new KeyValuePair<string, string>("keyOne", "valueOne");
        private KeyValuePair<string, string> secondPair = new KeyValuePair<string, string>("keyTwo", "valueTwo");

        [OneTimeSetUp]
        public async Task Setup()
        {
            IManager service = new Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName);

            var addedFolder = await service.AddFolderAsync(appFolder);
            var appSettingsFolder = await service.AddFolderAsync(EagerAdapter.AppSettingsName);
            await appSettingsFolder.AddAsync(firstPair.Key, firstPair.Value);
            await appSettingsFolder.AddAsync(secondPair.Key, secondPair.Value);

            var connectionStringFolder = await service.AddFolderAsync(EagerAdapter.ConnectionStringsName);
            await connectionStringFolder.AddAsync(firstPair.Key, firstPair.Value);
            await connectionStringFolder.AddAsync(secondPair.Key, secondPair.Value);
        }

        [Test]
        public async Task I_d_like_to_get_specific_appSetting_using_lazy_adapter()
        {
            //given 
            IReadOnly readOnlyService = new Manager(
               InputData.HostName,
               InputData.Port,
               InputData.ServiceHostName);

            var lazyAdapter = new LazyAdapter(readOnlyService);

            //when
            var appSettingsValueOfTheKey = lazyAdapter.AppSettings(firstPair.Key);
            var appSettingsValueOfTheSecondKey = lazyAdapter.AppSettings(secondPair.Key);

            //then
            Assert.AreEqual(appSettingsValueOfTheKey, firstPair.Value);
            Assert.AreEqual(appSettingsValueOfTheSecondKey, secondPair.Value);
        }

        [Test]
        public async Task I_d_like_to_get_specific_connectionString_using_lazy_adapter()
        {
            //given 
            IReadOnly readOnlyService = new Manager(
               InputData.HostName,
               InputData.Port,
               InputData.ServiceHostName);

            var lazyAdapter = new LazyAdapter(readOnlyService);

            //when
            var appSettingsValueOfTheKey = lazyAdapter.ConnectionStrings(firstPair.Key);
            var appSettingsValueOfTheSecondKey = lazyAdapter.ConnectionStrings(secondPair.Key);

            //then
            Assert.AreEqual(appSettingsValueOfTheKey, firstPair.Value);
            Assert.AreEqual(appSettingsValueOfTheSecondKey, secondPair.Value);
        }
    }
}