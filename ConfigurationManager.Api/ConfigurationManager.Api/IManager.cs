using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public interface IManager
    {
        Task<bool> AddFolderAsync(string key);
        Task<string> GetAsync(string key);
        Task<bool> AddAsync(string key, string value, bool toMainFolder = false);
        Task<bool> CleanInstanceAsync();
        Task<bool> RemoveAsync(string key);
        bool IsConnected();
    }
}