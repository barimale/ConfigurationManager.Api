using Consul;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public class Manager : IManager, IFolderPerspective
    {
        private readonly string MainFolder;
        private readonly string HostName;
        private readonly int Port;
        private readonly string ServiceHostName;
        private Manager Parent;

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
        }

        public Manager(string hostname, int port, string serviceHostName, string mainFolder, Manager parent)
            : this(hostname, port, serviceHostName, mainFolder)
        {
            Parent = parent;
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

        public async Task<IFolderPerspective> AddFolderAsync(string folderName)
        {
            try
            {
                if(!folderName.EndsWith("/"))
                {
                    folderName = string.Concat(folderName, "/");
                }

                var pair = new KVPair(folderName)
                {
                    Value = Encoding.UTF8.GetBytes(string.Empty)
                };

                var result = await _client.KV.Put(pair);

                return result.Response ? new Manager(HostName, Port, ServiceHostName, folderName, this) : throw new Exception();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveFolderAsync(IFolderPerspective service)
        {
            try
            {
                var path = service.GetLocationPath();

                return await RemoveAsync(path);
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

        public string GetLocationPath()
        {
            if (HasParent())
            {
                var subPath = Parent.GetLocationPath();
                return subPath != null ? string.Concat(subPath, "/", MainFolder) : MainFolder;
            }

            return MainFolder;
        }

        public bool IsConnected()
        {
            return _client != null;
        }

        private bool HasParent()
        {
            return Parent != null;
        }

        Task<string> IFolderPerspective.GetAsync(string key)
        {
            var finalKey = string.Concat(GetLocationPath(), "/", key);
            return GetAsync(key);
        }

        Task<bool> IFolderPerspective.AddAsync(string key, string value)
        {
            var finalKey = string.Concat(GetLocationPath(), "/", key);
            return AddAsync(finalKey, value);
        }

        Task<bool> IFolderPerspective.RemoveAsync(string key)
        {
            var finalKey = string.Concat(GetLocationPath(), "/", key);
            return RemoveAsync(finalKey);
        }
    }
}