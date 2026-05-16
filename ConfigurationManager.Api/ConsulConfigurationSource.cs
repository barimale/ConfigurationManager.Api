using Microsoft.Extensions.Configuration;

namespace ConfigurationManager.Api
{
    public class ConsulConfigurationSource : IConfigurationSource
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string ServiceHostName { get; set; }
        public string MainFolder { get; set; }
        public bool Optional { get; set; }
        public int ReloadDelay { get; set; }

        public ConsulConfigurationSource(string hostName, int port, string serviceHostName = "", string mainFolder = "")
        {
            HostName = hostName;
            Port = port;
            ServiceHostName = serviceHostName;
            MainFolder = mainFolder;
            Optional = false;
            ReloadDelay = 3000; // Default reload delay in milliseconds
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConsulConfigurationProvider(this);
        }
    }
}
