using Prism.Ioc;

namespace FEntwumS.CrossCompiler.Assistents;

public class TransferredContainerProvider
{
    public static IContainerProvider containerProvider;

    public TransferredContainerProvider(IContainerProvider pContainerProvider)
    {
        containerProvider = pContainerProvider;
    }

    public static T getService<T>()
    {
      return containerProvider.Resolve<T>();  
    }
}