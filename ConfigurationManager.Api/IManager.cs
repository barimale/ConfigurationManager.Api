using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public interface IManager
    {
        Task<IFolderPerspective> AddFolderAsync(string name);
        Task<IFolderPerspective> GetFolderAsync(string name);
        Task<string> GetAsync(string key);
        Task<bool> AddAsync(string key, string value);
        Task<bool> RemoveAsync(string key);
        bool IsConnected();
    }
}