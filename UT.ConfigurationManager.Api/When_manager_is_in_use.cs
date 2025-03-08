using ConfigurationManager.Api;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UT.ConfigurationManager.Api
{
    public class When_manager_is_in_use
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
            ClassicAssert.AreEqual(true, result);
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
            ClassicAssert.AreEqual(true, result);
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
            ClassicAssert.AreEqual(true, isConnected);
            ClassicAssert.AreEqual(true, isAdded);
            ClassicAssert.AreEqual(getValue, "bar");
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
            ClassicAssert.AreEqual(allOfThem.Count, 2);
            ClassicAssert.AreEqual(true,allOfThem.ContainsKey(firstPair.Key));
            ClassicAssert.AreEqual(true,allOfThem.ContainsKey(secondPair.Key));
            ClassicAssert.AreEqual(true,allOfThem.TryGetValue(firstPair.Key, out string firstResult));
            ClassicAssert.AreEqual(firstResult, firstPair.Value);
            ClassicAssert.AreEqual(true,allOfThem.TryGetValue(secondPair.Key, out string secondResult));
            ClassicAssert.AreEqual(secondResult, secondPair.Value);
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
            ClassicAssert.AreEqual(true,isConnected);
            ClassicAssert.NotNull(addedFolder);

            //and then
            var isGet = await service.GetFolderAsync(folderName);
            ClassicAssert.NotNull(isGet);

            //and then
            var isRemoved = await service.RemoveFolderAsync(addedFolder);
            ClassicAssert.IsTrue(isRemoved);

            //and then
            var isGetAgain = await service.GetFolderAsync(folderName);
            ClassicAssert.Null(isGetAgain);
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
            ClassicAssert.NotNull(isAdded);

            //and then
            var allFolderRemoved = await service.RemoveFolderAsync(addedFolder);
            ClassicAssert.IsTrue(allFolderRemoved);
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
            ClassicAssert.IsTrue(isKeyAdded);

            //and then
            var isRemoved = await addedFolder.RemoveAsync(key);
            ClassicAssert.IsTrue(isRemoved);

            //and then
            var isFolderRemoved = await service.RemoveFolderAsync(addedFolder);
            ClassicAssert.IsTrue(isFolderRemoved);
        }
    }
}