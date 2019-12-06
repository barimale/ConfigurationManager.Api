using System.Collections.Specialized;
using System.Linq;

namespace ConfigurationManager.Api.Helper
{
    public class EagerAdapter : BaseAdapter
    {
        public EagerAdapter(IReadOnly manager)
        {
            _manager = manager;
        }

        public NameValueCollection AppSettings()
        {
            var folder = _manager.GetFolderAsync(AppSettingsName).Result;

            return folder.AllKeyValuePairsAsync()
                .Result
                .Aggregate(new NameValueCollection(),
                    (seed, current) => {
                    seed.Add(current.Key, current.Value);
                    return seed;
                    }); 
        }

        public NameValueCollection ConnectionStrings()
        {
            var folder = _manager.GetFolderAsync(ConnectionStringsName).Result;

            return folder.AllKeyValuePairsAsync()
                .Result
                .Aggregate(new NameValueCollection(),
                    (seed, current) => {
                        seed.Add(current.Key, current.Value);
                        return seed;
                    });
        }
    }
}
