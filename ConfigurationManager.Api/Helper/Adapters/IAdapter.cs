namespace ConfigurationManager.Api.Helper.Adapters
{
    public interface IAdapter
    {
        string AppSettings(string key);
        string ConnectionStrings(string key);
    }
}