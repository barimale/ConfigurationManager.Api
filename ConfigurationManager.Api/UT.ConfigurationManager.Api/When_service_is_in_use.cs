using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace UT.ConfigurationManager.Api
{
    public class When_service_is_in_use
    {
        [SetUp]
        public void Setup()
        {
            //intentionally left blank
        }

        [Test]
        public void I_d_like_to_connect_to_consul_instance()
        {
            //given 
            var service = new global::ConfigurationManager.Api.Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName);

            //when
            var result = service.IsConnected();

            //than
            Assert.IsTrue(result);
        }

        [Test]
        public async Task I_d_like_to_add_dummy_key()
        {
            //given 
            var service = new global::ConfigurationManager.Api.Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName,
                Guid.NewGuid().ToString());

            var isConnected = service.IsConnected();

            //when
            var isAdded = await service.AddAsync("foo", "bar");
            var getValue = await service.GetAsync("foo");

            //than
            Assert.IsTrue(isConnected);
            Assert.IsTrue(isAdded);
            Assert.AreEqual(getValue, "bar");
        }

        public async Task I_d_like_to_add_dummy_key_to_main_folder()
        {
            //given 
            var service = new global::ConfigurationManager.Api.Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName,
                Guid.NewGuid().ToString());

            //when
            var isAdded = await service.AddAsync("foo", "bar", true);

            //than
            Assert.IsTrue(isAdded);
        }

        [Test]
        public async Task I_d_like_to_add_unique_folder()
        {
            //given 
            var folderName = Guid.NewGuid().ToString();

            var service = new global::ConfigurationManager.Api.Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName);

            var isConnected = service.IsConnected();

            //when
            var isAdded = await service.AddFolderAsync(folderName);

            //than
            Assert.IsTrue(isConnected);
            Assert.IsTrue(isAdded);

            //and than
            await service.RemoveFolderAsync(folderName);
        }

        [Test]
        public async Task I_d_like_to_remove_dummy_key()
        {
            //given 
            var service = new global::ConfigurationManager.Api.Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName);

            //when
            var isAdded = await service.AddAsync("foo", "bar");
            var getValue = await service.GetAsync("foo");
            var isRemoved = await service.RemoveAsync("foo");
            var tryGetRemoved = await service.GetAsync("foo");

            //than
            Assert.IsTrue(isAdded);
            Assert.AreEqual(getValue, "bar");
            Assert.IsTrue(isRemoved);
            Assert.AreEqual(tryGetRemoved, "");
        }
    }
}