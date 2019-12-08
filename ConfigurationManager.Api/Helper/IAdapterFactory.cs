using ConfigurationManager.Api.Helper.Adapters;

namespace ConfigurationManager.Api.Helper
{
    public interface IAdapterFactory
    {
        T GetAdapter<T>(IReadOnly manager) where T : BaseAdapter;
    }
}