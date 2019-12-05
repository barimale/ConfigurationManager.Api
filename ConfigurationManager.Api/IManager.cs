using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public interface IManager : IReadOnly
    {
        Task<IManager> AddFolderAsync(string name);
        Task<bool> RemoveFolderAsync(IManager service);
        Task<bool> AddAsync(string key, string value);
        Task<bool> RemoveAsync(string key);
        new Task<IManager> GetFolderAsync(string name);
    }
}