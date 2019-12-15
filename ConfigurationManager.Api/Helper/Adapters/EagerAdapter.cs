using System.Collections.Specialized;
using System.Linq;

namespace ConfigurationManager.Api.Helper.Adapters
{
    public class EagerAdapter : BaseAdapter, IAdapter
    {
        public NameValueCollection InnerAppSettings { private set; get; }
        public NameValueCollection InnerConnectionStrings { private set; get; }

        public EagerAdapter(IReadOnly manager)
        {
            _manager = manager;

            InnerAppSettings = LoadAppSettings();
            InnerConnectionStrings = LoadConnectionStrings();
        }

        public string AppSettings(string key)
        {
            return InnerAppSettings[key];
        }

        public string ConnectionStrings(string key)
        {
            return InnerConnectionStrings[key];
        }

        private NameValueCollection LoadAppSettings()
        {
            var folder = _manager.GetFolderAsync(AppSettingsName).Result;

            return folder.AllKeyValuePairsAsync()
                .Result
                .Aggregate(new NameValueCollection(),
                    (seed, current) =>
                    {
                        seed.Add(current.Key, current.Value);
                        return seed;
                    });
        }

        private NameValueCollection LoadConnectionStrings()
        {
            var folder = _manager.GetFolderAsync(ConnectionStringsName).Result;

            return folder.AllKeyValuePairsAsync()
                .Result
                .Aggregate(new NameValueCollection(),
                    (seed, current) =>
                    {
                        seed.Add(current.Key, current.Value);
                        return seed;
                    });
        }
    }
}