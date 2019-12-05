using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigurationManager.Api
{
    public interface IReadOnly
    {
        Task<Dictionary<string, string>> AllKeyValuePairsAsync(CancellationToken token = default);
        Task<string> GetAsync(string key);
        Task<IReadOnly> GetFolderAsync(string name);
        string GetLocationPath();
        bool IsConnected();
    }
}
