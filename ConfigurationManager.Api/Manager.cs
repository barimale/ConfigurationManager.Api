using Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public class Manager : IManager, IReadOnly
    {
        private readonly string MainFolder = string.Empty;
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
            if(!MainFolder.EndsWith("/"))
            {
                MainFolder += "/";
            }
        }

        private Manager(string hostname, int port, string serviceHostName, string mainFolder, Manager parent)
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

        public async Task<IManager> AddFolderAsync(string name)
        {
            try
            {
                name = GetAbsolutName(name);

                var pair = new KVPair(name)
                {
                    Value = Encoding.UTF8.GetBytes(string.Empty)
                };

                var result = await _client.KV.Put(pair);

                return result.Response ? new Manager(HostName, Port, ServiceHostName, name, this) : throw new Exception();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetAbsolutName(string name)
        {
            name = string.Concat(this.GetLocationPath(), name);

            if (!name.EndsWith("/"))
            {
                name = string.Concat(name, "/");
            }

            if (name.StartsWith("/"))
            {
                name = name.TrimStart('/');
            }

            return name;
        }

        public async Task<IManager> GetFolderAsync(string name, CancellationToken token)
        {
            try
            {
                name = GetAbsolutName(name);

                var alreadyExist = await _client.KV.Get(name, token);
                if (alreadyExist == null || alreadyExist.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return null;
                }

                return new Manager(HostName, Port, ServiceHostName, name, this);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> RemoveFolderAsync(IManager service)
        {
            try
            {
                var path = service.GetLocationPath();

                return await RemoveAsync(path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                var result = await _client.KV.Delete(key);

                return result.Response;
            }
            catch (Exception ex)
            {
                throw ex;
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetLocationPath()
        {
            if (HasParent())
            {
                var subPath = Parent.GetLocationPath();
                return !string.IsNullOrEmpty(subPath) ? MainFolder : MainFolder;
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

        Task<string> IReadOnly.GetAsync(string key)
        {
            var finalKey = string.Concat(GetLocationPath(), "/", key);
            return GetAsync(finalKey);
        }

        Task<bool> IManager.AddAsync(string key, string value)
        {
            var finalKey = string.Concat(GetLocationPath(), "/", key);
            return AddAsync(finalKey, value);
        }

        Task<bool> IManager.RemoveAsync(string key)
        {
            var finalKey = string.Concat(GetLocationPath(), "/", key);
            return RemoveAsync(finalKey);
        }

        async Task<IReadOnly> IReadOnly.GetFolderAsync(string name, CancellationToken token)
        {
            return await GetFolderAsync(name, token) as IReadOnly;
        }

        public async Task<Dictionary<string, string>> AllKeyValuePairsAsync(CancellationToken token = default)
        {
            try
            {
                var finalPath = GetLocationPath();
                var allOfThem = await _client.KV.Keys(finalPath, token);

                var keyValues = new Dictionary<string, string>();
                foreach (var key in allOfThem.Response.ToList())
                {
                    if (key.EndsWith("/"))
                        continue;

                    var result = await GetAsync(key);
                    var index = key.LastIndexOf('/');
                    keyValues.Add(key.Substring(index+1, key.Length - index -1), result);
                }

                return keyValues;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}