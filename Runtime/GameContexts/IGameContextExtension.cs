namespace Flexy.Core.GameContexts;

public interface IGameContextExtension
{
	void	SetParent				( GameContext parent );
	void	RegisterInitialServices	( Dictionary<Type, object> registeredServicesDict );
	T?		GetService<T>			( ) where T : class;
	void	SetService<T>			( Type serviceType, T service, Boolean replace ) where T : class;
}