using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public interface IManager
    {
        Task<string> GetAsync(string key);
        Task<bool> AddAsync(string key, string value);
        Task<bool> CleanInstanceAsync();
        Task<bool> RemoveAsync(string key);
        bool IsConnected();
    }
}