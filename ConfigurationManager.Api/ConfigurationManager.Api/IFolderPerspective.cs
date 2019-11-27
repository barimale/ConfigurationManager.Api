using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public interface IFolderPerspective
    {
        Task<string> GetAsync(string key);
        Task<bool> AddAsync(string key, string value);
        Task<bool> RemoveAsync(string key);
        string GetLocationPath();
    }
}
