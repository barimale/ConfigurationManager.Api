using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public interface IManager
    {
        Task<bool> AddAsync(string key, string value);
        Task<bool> CleanInstanceAsync();
        Task<bool> RemoveAsync(string key, string value);
        Task<bool> ConnectAsync();
        Task<bool> ConnectAsync(string url);
    }
}