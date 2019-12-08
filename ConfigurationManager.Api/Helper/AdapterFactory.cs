using ConfigurationManager.Api.Helper.Adapters;
using Ninject;
using Ninject.Syntax;

namespace ConfigurationManager.Api.Helper
{
    public class AdapterFactory : IAdapterFactory
    {
        private readonly IResolutionRoot _resolutionRoot;

        public AdapterFactory(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public T GetAdapter<T>(IReadOnly manager)
            where T : BaseAdapter
        {
            var parameter = new Ninject.Parameters.ConstructorArgument("manager", manager);
            return _resolutionRoot.Get<T>(parameter);
        }
    }
}
