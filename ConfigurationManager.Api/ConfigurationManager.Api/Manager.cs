using Consul;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public class Manager : IManager
    {
        private readonly string MainFolder;
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
                var address = string.Concat(HostName, ":", Port);
                consulConfig.Address = new Uri(address);
            }, null, handlerOverride =>
            {
                handlerOverride.Proxy = null;
                handlerOverride.UseProxy = false;
            });
        }

        public Manager(string hostname, int port, string serviceHostName, string mainFolder)
            : this(hostname, port, serviceHostName)
        {
            MainFolder = mainFolder;
            AddFolderAsync(MainFolder);
        }

        public async Task<bool> AddAsync(string key, string value, bool toMainFolder = false)
        {
            try
            {
                if(toMainFolder)
                {
                    var a = await _client.KV.List(MainFolder);
                }

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

        public async Task<bool> AddFolderAsync(string key)
        {
            try
            {
                if(!key.EndsWith("/"))
                {
                    key = string.Concat(key, "/");
                }

                var pair = new KVPair(key)
                {
                    Value = Encoding.UTF8.GetBytes(string.Empty)
                };

                var result = await _client.KV.Put(pair);

                return result.Response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveFolderAsync(string key)
        {
            try
            {
                if (!key.EndsWith("/"))
                {
                    key = string.Concat(key, "/");
                }

                return await RemoveAsync(key);
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
                var result = await _client.KV.Delete(key);

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
                var res = await _client.KV.Get(key);

                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                { 
                    return Encoding.UTF8.GetString(res.Response.Value);
                }

                return string.Empty;
            }
            catch (Exception)
            {
                throw;
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