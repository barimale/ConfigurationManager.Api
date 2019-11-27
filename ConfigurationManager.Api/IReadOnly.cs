using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public interface IReadOnly
    {
        Task<string> GetAsync(string key);
        Task<IReadOnly> GetFolderAsync(string name);
        string GetLocationPath();
        bool IsConnected();
    }
}
