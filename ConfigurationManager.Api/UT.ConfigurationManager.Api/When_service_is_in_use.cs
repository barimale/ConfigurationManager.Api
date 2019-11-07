using NUnit.Framework;
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
                InputData.ServiceHostName);

            var isConnected = service.IsConnected();

            //when
            var isAdded = await service.AddAsync("foo", "bar");
            var getValue = await service.GetAsync("foo");

            //than
            Assert.IsTrue(isConnected);
            Assert.IsTrue(isAdded);
            Assert.AreEqual(getValue, "bar");
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