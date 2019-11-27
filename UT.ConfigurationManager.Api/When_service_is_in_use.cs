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

            //then
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

            //then
            Assert.IsTrue(isConnected);
            Assert.IsTrue(isAdded);
            Assert.AreEqual(getValue, "bar");
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
            var addedFolder = await service.AddFolderAsync(folderName);

            //then
            Assert.IsTrue(isConnected);
            Assert.NotNull(addedFolder);

            //and then
            var isGet = await service.GetFolderAsync(folderName);
            Assert.NotNull(isGet);

            //and then
            var isRemoved = await service.RemoveFolderAsync(addedFolder);
            Assert.IsTrue(isRemoved);

            //and then
            var isGetAgain = await service.GetFolderAsync(folderName);
            Assert.Null(isGetAgain);
        }

        [Test]
        public async Task I_d_like_to_add_key_to_the_specific_folder()
        {
            //given 
            var folderName = Guid.NewGuid().ToString();

            var service = new global::ConfigurationManager.Api.Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName);

            var addedFolder = await service.AddFolderAsync(folderName);
            var key = "foo";

            //when
            var isKeyAdded = await addedFolder.AddAsync(key, "bar");

            //then
            Assert.IsTrue(isKeyAdded);

            //and then
            var isRemoved = await addedFolder.RemoveAsync(key);
            Assert.IsTrue(isRemoved);

            //and then
            var isFolderRemoved = await service.RemoveFolderAsync(addedFolder);
            Assert.IsTrue(isFolderRemoved);
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