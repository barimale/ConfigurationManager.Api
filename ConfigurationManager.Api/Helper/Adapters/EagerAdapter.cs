using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace ConfigurationManager.Api.Helper.Adapters
{
    public class EagerAdapter : BaseAdapter, IAdapter
    {
        private CancellationTokenSource tokenSource = new CancellationTokenSource(5000);
        public NameValueCollection InnerAppSettings { private set; get; }
        public ConnectionStringSettingsCollection InnerConnectionStrings { private set; get; }

        public EagerAdapter(IReadOnly manager)
        {
            _manager = manager;

            InnerAppSettings = LoadAppSettings(tokenSource.Token);
            InnerConnectionStrings = LoadConnectionStrings(tokenSource.Token);
        }

        public string AppSettings(string key)
        {
            return InnerAppSettings[key];
        }

        public string ConnectionStrings(string key)
        {
            return InnerConnectionStrings[key].ConnectionString;
        }

        private NameValueCollection LoadAppSettings(CancellationToken token)
        {
            try
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
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        private ConnectionStringSettingsCollection LoadConnectionStrings(CancellationToken token)
        {
            try
            {
                var folder = _manager.GetFolderAsync(ConnectionStringsName).Result;

                return folder.AllKeyValuePairsAsync()
                    .Result
                    .Aggregate(new ConnectionStringSettingsCollection(),
                        (seed, current) =>
                        {
                            var setting = new ConnectionStringSettings
                            {
                                ConnectionString = current.Value,
                                Name = current.Key
                            };

                            seed.Add(setting);
                            return seed;
                        });
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }
}