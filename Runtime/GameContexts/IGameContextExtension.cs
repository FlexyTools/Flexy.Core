namespace Flexy.Core.GameContexts;

public interface IGameContextExtension
{
	void SetParent(GameContext parent);
	void RegisterInitialServices( Dictionary<Type, Object> registeredServicesDict );
	T? GetService<T>() where T : class;
	void SetService<T>(Type serviceType, T service) where T : class;
}