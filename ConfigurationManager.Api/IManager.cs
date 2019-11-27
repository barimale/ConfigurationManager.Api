using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public interface IManager
    {
        Task<IFolderPerspective> AddFolderAsync(string key);
        Task<string> GetAsync(string key);
        Task<bool> AddAsync(string key, string value);
        Task<bool> RemoveAsync(string key);
        bool IsConnected();
    }
}