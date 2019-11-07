using NUnit.Framework;

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
            var service = new global::ConfigurationManager.Api.Manager(InputData.Url);

            //when


            //than
            Assert.Pass();
        }
    }
}