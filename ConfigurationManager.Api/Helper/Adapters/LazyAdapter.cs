using System.Collections.Specialized;
using System.Configuration;

namespace ConfigurationManager.Api.Helper.Adapters
{
    public class LazyAdapter : BaseAdapter, IAdapter
    {
        public ConnectionStringSettingsCollection InnerConnectionStrings => throw new System.NotSupportedException();
        public NameValueCollection InnerAppSettings => throw new System.NotSupportedException();

        public LazyAdapter(IReadOnly manager)
        {
            _manager = manager;
        }

        public string AppSettings(string key)
        {
            var folder = _manager.GetFolderAsync(AppSettingsName).Result;

            return folder.GetAsync(key).Result;
        }

        public string ConnectionStrings(string key)
        {
            var folder = _manager.GetFolderAsync(ConnectionStringsName).Result;

            return folder.GetAsync(key).Result;
        }
    }
}
