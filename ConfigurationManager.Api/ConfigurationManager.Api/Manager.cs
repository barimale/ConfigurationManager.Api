using Consul;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public class Manager : IManager
    {
        private readonly string HostName;
        private readonly int Port;
        private readonly string ServiceHostName;

        private IConsulClient _client;

        public Manager(string hostname, int port, string serviceHostName = "")
        {
            HostName = hostname;
            Port = port;
            ServiceHostName = serviceHostName;

            _client = new ConsulClient(consulConfig =>
            {
                var address = string.Concat(hostname, ":", Port);
                consulConfig.Address = new Uri(address);
            }, null, handlerOverride =>
            {
                handlerOverride.Proxy = null;
                handlerOverride.UseProxy = false;
            });
        }

        public async Task<bool> AddAsync(string key, string value)
        {
            try
            {
                var pair = new KVPair(key)
                {
                    Value = Encoding.UTF8.GetBytes(value)
                };

                var result = await _client.KV.Put(pair);

                return result.Response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                var result = await _client.ACL.Destroy(key);

                return result.Response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> GetAsync(string key)
        {
            try
            {
                var str = string.Empty;
                var res = await _client.KV.Get(key);

                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                { 
                    str = Encoding.UTF8.GetString(res.Response.Value);
                }

                return str;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<bool> CleanInstanceAsync()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            return _client != null;
        }
    }
}
