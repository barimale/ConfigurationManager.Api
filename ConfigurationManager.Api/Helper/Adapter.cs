namespace ConfigurationManager.Api.Helper
{
    public class Adapter
    {
        private readonly IReadOnly _manager;
        private const string AppSettingsName = "AppSettings";
        private const string ConnectionStringsName = "ConnectionStrings";

        public Adapter(IReadOnly manager)
        {
            _manager = manager;
        }

        public string AppSettings(string key)
        {
            var folder = _manager.GetFolderAsync(AppSettingsName).Result;

            return folder.GetAsync(key).Result;
        }

        public string ConnectionString(string key)
        {
            var folder = _manager.GetFolderAsync(ConnectionStringsName).Result;

            return folder.GetAsync(key).Result;
        }
    }
}
