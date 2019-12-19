using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace ConfigurationManager.Api.Helper.Adapters
{
    public class EagerAdapter : BaseAdapter, IAdapter
    {
        private CancellationTokenSource tokenSource = new CancellationTokenSource(30000);
        private NameValueCollection innerAppSettings;
        private ConnectionStringSettingsCollection innerConnectionStrings;

        public EagerAdapter(IReadOnly manager)
        {
            _manager = manager;
        }

        public EagerAdapter(IReadOnly manager, int cancellationTimeInMilliseconds)
        : this(manager)
        {
            tokenSource = new CancellationTokenSource(cancellationTimeInMilliseconds);
        }

        public ConnectionStringSettingsCollection InnerConnectionStrings
        {
            get
            {
                if (innerConnectionStrings == null)
                {
                    innerConnectionStrings = LoadConnectionStrings(tokenSource.Token);
                }

                return innerConnectionStrings;
            }
        }

        public NameValueCollection InnerAppSettings
        {
            get
            {
                if (innerAppSettings == null)
                {
                    innerAppSettings = LoadAppSettings(tokenSource.Token);
                }

                return innerAppSettings;
            }
        }

        public string AppSettings(string key)
        {
            return InnerAppSettings[key];
        }

        public string ConnectionStrings(string key)
        {
            return InnerConnectionStrings[key].ConnectionString;
        }

        public bool Initialize()
        {
            try
            {
                innerConnectionStrings = LoadConnectionStrings(tokenSource.Token);
                innerAppSettings = LoadAppSettings(tokenSource.Token);

                return true;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        private NameValueCollection LoadAppSettings(CancellationToken token)
        {
            try
            {
                var folder = _manager.GetFolderAsync(AppSettingsName, token).Result;

                return folder.AllKeyValuePairsAsync(token)
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
                var folder = _manager.GetFolderAsync(ConnectionStringsName, token).Result;

                return folder.AllKeyValuePairsAsync(token)
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