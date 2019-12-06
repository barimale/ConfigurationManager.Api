namespace ConfigurationManager.Api.Helper
{
    public abstract class BaseAdapter
    {
        public const string AppSettingsName = "AppSettings";
        public const string ConnectionStringsName = "ConnectionStrings";

        protected IReadOnly _manager;
    }
}
