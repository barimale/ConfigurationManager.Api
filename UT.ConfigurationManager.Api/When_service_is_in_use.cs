using ConfigurationManager.Api;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
        public void I_d_like_to_connect_to_consul_instance_as_read_write_manager()
        {
            //given 
            IManager service = new Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName);

            //when
            var result = service.IsConnected();

            //then
            Assert.IsTrue(result);
        }

        [Test]
        public void I_d_like_to_connect_to_consul_instance_as_read_only_client()
        {
            //given 
            IReadOnly service = new Manager(
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
            IManager service = new Manager(
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
        public async Task I_d_like_to_get_all_key_values_from_unique_folder()
        {
            //given 
            var folderName = Guid.NewGuid().ToString();

            IManager service = new Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName);

            var addedFolder = await service.AddFolderAsync(folderName);
            var firstPair = new KeyValuePair<string, string>("keyOne", "valueOne");
            var secondPair = new KeyValuePair<string, string>("keyTwo", "valueTwo");
            await addedFolder.AddAsync(firstPair.Key, firstPair.Value);
            await addedFolder.AddAsync(secondPair.Key, secondPair.Value);

            //when
            var allOfThem = await addedFolder.AllKeyValuePairsAsync();

            //then
            Assert.AreEqual(allOfThem.Count, 2);
            Assert.IsTrue(allOfThem.ContainsKey(firstPair.Key));
            Assert.IsTrue(allOfThem.ContainsKey(secondPair.Key));
            Assert.IsTrue(allOfThem.TryGetValue(firstPair.Key, out string firstResult));
            Assert.AreEqual(firstResult, firstPair.Value);
            Assert.IsTrue(allOfThem.TryGetValue(secondPair.Key, out string secondResult));
            Assert.AreEqual(secondResult, secondPair.Value);
        }

        [Test]
        public async Task I_d_like_to_add_unique_folder()
        {
            //given 
            var folderName = Guid.NewGuid().ToString();

            IManager service = new Manager(
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
        public async Task I_d_like_to_add_unique_folder_to_existed_folder()
        {
            //given 
            var folderName = Guid.NewGuid().ToString();

            IManager service = new Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName);

            var addedFolder = await service.AddFolderAsync(folderName);
            var isGet = await service.GetFolderAsync(folderName);

            //when
            var newFolderName = Guid.NewGuid().ToString();
            var isAdded = await isGet.AddFolderAsync(newFolderName);

            //then
            Assert.NotNull(isAdded);

            //and then
            var allFolderRemoved = await service.RemoveFolderAsync(addedFolder);
            Assert.IsTrue(allFolderRemoved);
        }

        [Test]
        public async Task I_d_like_to_add_key_to_the_specific_folder()
        {
            //given 
            var folderName = Guid.NewGuid().ToString();

            IManager service = new Manager(
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
            IManager service = new Manager(
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