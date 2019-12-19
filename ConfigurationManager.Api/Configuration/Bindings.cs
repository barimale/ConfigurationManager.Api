using Ninject.Modules;

namespace ConfigurationManager.Api.Bindings
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<IManager, IReadOnly>().To<Manager>();
        }
    }
}
