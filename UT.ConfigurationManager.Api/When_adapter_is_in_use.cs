using ConfigurationManager.Api;
using ConfigurationManager.Api.Bindings;
using ConfigurationManager.Api.Helper;
using ConfigurationManager.Api.Helper.Adapters;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UT.ConfigurationManager.Api
{
    public class When_adapter_is_in_use
    {
        private readonly StandardKernel _kernel;
        private IManager service;
        private IManager addedFolder;
        private string appFolder = Guid.NewGuid().ToString().ToUpper();
        private KeyValuePair<string, string> firstPair = new KeyValuePair<string, string>("keyOne", "valueOne");
        private KeyValuePair<string, string> secondPair = new KeyValuePair<string, string>("keyTwo", "valueTwo");

        public When_adapter_is_in_use()
        {
            if(_kernel == null)
                _kernel = new StandardKernel(new Bindings());
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            service.RemoveFolderAsync(addedFolder);
            _kernel.Dispose();
        }

        [OneTimeSetUp]
        public async Task Setup()
        {
            service = new Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName);

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
                appFolder);

            var factory = _kernel.Get<IAdapterFactory>();
            var lazyAdapter = factory.GetAdapter<LazyAdapter>(readOnlyService);

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
               InputData.ServiceHostName,
               appFolder);

            var lazyAdapter = new LazyAdapter(readOnlyService);

            //when
            var appSettingsValueOfTheKey = lazyAdapter.ConnectionStrings(firstPair.Key);
            var appSettingsValueOfTheSecondKey = lazyAdapter.ConnectionStrings(secondPair.Key);

            //then
            Assert.AreEqual(appSettingsValueOfTheKey, firstPair.Value);
            Assert.AreEqual(appSettingsValueOfTheSecondKey, secondPair.Value);
        }

        [Test]
        public async Task I_d_like_to_get_specific_appSetting_using_eager_adapter()
        {
            //given 
            IReadOnly readOnlyService = new Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName,
                appFolder);

            var eagerAdapter = new EagerAdapter(readOnlyService);

            //when
            var appSettingsValueOfTheKey = eagerAdapter.AppSettings(firstPair.Key);
            var appSettingsValueOfTheSecondKey = eagerAdapter.AppSettings(secondPair.Key);

            //then
            Assert.AreEqual(appSettingsValueOfTheKey, firstPair.Value);
            Assert.AreEqual(appSettingsValueOfTheSecondKey, secondPair.Value);
        }

        [Test]
        public async Task I_d_like_to_get_specific_connectionString_using_eager_adapter()
        {
            //given 
            IReadOnly readOnlyService = new Manager(
               InputData.HostName,
               InputData.Port,
               InputData.ServiceHostName,
               appFolder);

            var eagerAdapter = new EagerAdapter(readOnlyService);

            //when
            var appSettingsValueOfTheKey = eagerAdapter.ConnectionStrings(firstPair.Key);
            var appSettingsValueOfTheSecondKey = eagerAdapter.ConnectionStrings(secondPair.Key);

            //then
            Assert.AreEqual(appSettingsValueOfTheKey, firstPair.Value);
            Assert.AreEqual(appSettingsValueOfTheSecondKey, secondPair.Value);
        }
    }
}