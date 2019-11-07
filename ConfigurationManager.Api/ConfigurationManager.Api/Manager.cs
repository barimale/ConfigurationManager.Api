using System.Threading.Tasks;
using System;

namespace ConfigurationManager.Api
{
    public class Manager : IManager
    {
        private string Url;

        //TODO: each application has to have its own instance of consul, which have to live as long
        // as application

        public Manager()
        {
            //intentionally left blank
        }

        public Manager(string url)
            :this()
        {
            Url = url;
            //TODO: create instance of Consul, or connect to your own instance
            //remove all keys and values
        }

        public async Task<bool> AddAsync(string key, string value)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveAsync(string key, string value)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RefreshAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConnectAsync(string url)
        {
            throw new NotImplementedException();
        }
    }
}
